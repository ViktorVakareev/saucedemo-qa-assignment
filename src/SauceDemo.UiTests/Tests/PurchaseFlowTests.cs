using SauceDemo.UiTests.Config;
using SauceDemo.UiTests.Infrastructure;
using SauceDemo.UiTests.Models;
using SauceDemo.UiTests.Pages;

namespace SauceDemo.UiTests.Tests;

[TestFixture]
[Category("UI")]
[Parallelizable(ParallelScope.Self)]
public sealed class PurchaseFlowTests : UiTestBase
{
    [Test]
    [Description("Assignment scenario: highest/lowest price in cart, remove highest, add last A-Z, checkout, logout.")]
    public async Task Standard_user_completes_an_order_after_changing_the_cart()
    {
        var customer = CheckoutCustomer.Load();
        InventoryPage inventory = null!;
        CartPage cart = null!;
        Product highest = null!, lowest = null!;
        string lastByName = null!;

        await Step("1-2. Get the standard user's credentials and log in", async () =>
        {
            var loginPage = await new LoginPage(Page).OpenAsync();
            var credentials = await CredentialsProvider.ResolveAsync(loginPage, Settings);
            TestContext.Progress.WriteLine($"  using {credentials}");
            inventory = await loginPage.LoginAsync(credentials);
            await Expect(inventory.Title).ToHaveTextAsync("Products");
        });

        await Step("3. Add the highest- and the lowest-priced product", async () =>
        {
            var products = await inventory.GetProductsAsync();
            highest = products.MaxBy(p => p.Price)!;
            lowest = products.MinBy(p => p.Price)!;
            Assert.That(highest.Price, Is.GreaterThan(lowest.Price), "the catalogue needs at least two different prices");
            TestContext.Progress.WriteLine($"  highest: {highest}, lowest: {lowest}");

            await inventory.AddToCartAsync(highest.Name);
            await inventory.AddToCartAsync(lowest.Name);
            await Expect(inventory.Header.CartBadge).ToHaveTextAsync("2");

            cart = await inventory.Header.OpenCartAsync();
            Assert.That(await cart.GetItemNamesAsync(), Is.EquivalentTo(new[] { highest.Name, lowest.Name }));
        });

        await Step("4. Remove the highest-priced product and check the cart again", async () =>
        {
            await cart.RemoveAsync(highest.Name);
            await Expect(cart.Items).ToHaveCountAsync(1);
            Assert.That(await cart.GetItemNamesAsync(), Is.EqualTo(new[] { lowest.Name }));
            await Expect(cart.Header.CartBadge).ToHaveTextAsync("1");
        });

        await Step("5. Sort by Name (A to Z) and add the last product", async () =>
        {
            inventory = await cart.ContinueShoppingAsync();
            await inventory.SortByAsync(SortOption.NameAToZ);

            var names = await inventory.GetProductNamesAsync();
            Assert.That(names, Is.Ordered.Using((IComparer<string>)StringComparer.OrdinalIgnoreCase), "list is sorted A to Z");
            lastByName = names[^1];
            Assert.That(lastByName, Is.Not.EqualTo(lowest.Name),
                "test-data assumption: the last product A-Z isn't already in the cart");

            await inventory.AddToCartAsync(lastByName);
            await Expect(inventory.Header.CartBadge).ToHaveTextAsync("2");
        });

        CheckoutOverviewPage overview = null!;
        await Step("6. Go to checkout", async () =>
        {
            cart = await inventory.Header.OpenCartAsync();
            Assert.That(await cart.GetItemNamesAsync(), Is.EquivalentTo(new[] { lowest.Name, lastByName }));
            var information = await cart.CheckoutAsync();
            overview = await information.ContinueAsync(customer);
        });

        CheckoutCompletePage complete = null!;
        await Step("7. Check the order summary and finish the order", async () =>
        {
            Assert.That(await overview.GetItemNamesAsync(), Is.EquivalentTo(new[] { lowest.Name, lastByName }));
            var linePrices = await overview.GetItemPricesAsync();
            var itemTotal = await overview.GetItemTotalAsync();
            var tax = await overview.GetTaxAsync();
            var total = await overview.GetTotalAsync();
            Assert.Multiple(() =>
            {
                Assert.That(itemTotal, Is.EqualTo(linePrices.Sum()), "item total = sum of line prices");
                Assert.That(total, Is.EqualTo(itemTotal + tax), "total = item total + tax");
            });
            complete = await overview.FinishAsync();
        });

        await Step("8. Verify the order is placed", async () =>
        {
            await Expect(complete.ConfirmationHeader).ToHaveTextAsync("Thank you for your order!");
            await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("checkout-complete"));
        });

        await Step("9. Verify the cart is empty", async () =>
        {
            await Expect(complete.Header.CartBadge).ToHaveCountAsync(0);
            cart = await complete.Header.OpenCartAsync();
            await Expect(cart.Items).ToHaveCountAsync(0);
        });

        await Step("10. Log out", async () =>
        {
            var loginPage = await cart.Header.LogoutAsync();
            await Expect(loginPage.LoginButton).ToBeVisibleAsync();

            // Logged out means the session is gone, not just that the login form is shown.
            await Page.GotoAsync("/inventory.html");
            await Expect(loginPage.Error).ToContainTextAsync("when you are logged in");
        });
    }
}
