using System.Text.RegularExpressions;
using SauceDemo.UiTests.Config;
using SauceDemo.UiTests.Infrastructure;
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

        var inventory = await Step("1-2. Get the standard user's credentials and log in", async () =>
        {
            var loginPage = await new LoginPage(Page).OpenAsync();
            var credentials = await CredentialsProvider.ResolveAsync(loginPage, Settings);
            TestContext.Progress.WriteLine($"  using {credentials}");

            var inventoryPage = await loginPage.LoginAsync(credentials);
            await Expect(inventoryPage.Title).ToHaveTextAsync("Products");
            return inventoryPage;
        });

        var (highest, lowest, cart) = await Step("3. Add the highest- and the lowest-priced product", async () =>
        {
            var products = await inventory.GetProductsAsync();
            var highestPriced = products.MaxBy(p => p.Price)!;
            var lowestPriced = products.MinBy(p => p.Price)!;
            Assert.That(highestPriced.Price, Is.GreaterThan(lowestPriced.Price), "the catalogue needs at least two different prices");
            TestContext.Progress.WriteLine($"  highest: {highestPriced}, lowest: {lowestPriced}");

            await inventory.AddToCartAsync(highestPriced.Name);
            await inventory.AddToCartAsync(lowestPriced.Name);
            await Expect(inventory.Header.CartBadge).ToHaveTextAsync("2");

            var cartPage = await inventory.Header.OpenCartAsync();
            await Expect(cartPage.Items).ToHaveCountAsync(2);   // wait for the cart to render before reading it
            Assert.That(await cartPage.GetItemNamesAsync(), Is.EquivalentTo(new[] { highestPriced.Name, lowestPriced.Name }));
            return (highestPriced, lowestPriced, cartPage);
        });

        await Step("4. Remove the highest-priced product and check the cart again", async () =>
        {
            await cart.RemoveAsync(highest.Name);
            await Expect(cart.Items).ToHaveCountAsync(1);
            Assert.That(await cart.GetItemNamesAsync(), Is.EqualTo(new[] { lowest.Name }));
            await Expect(cart.Header.CartBadge).ToHaveTextAsync("1");
        });

        var lastByName = await Step("5. Sort by Name (A to Z) and add the last product", async () =>
        {
            inventory = await cart.ContinueShoppingAsync();
            await inventory.SortByAsync(SortOption.NameAToZ);

            var names = await inventory.GetProductNamesAsync();
            Assert.That(names, Is.Ordered.Using((IComparer<string>)StringComparer.OrdinalIgnoreCase), "list is sorted A to Z");
            var last = names[^1];
            Assert.That(last, Is.Not.EqualTo(lowest.Name), "test-data assumption: the last product A-Z isn't already in the cart");

            await inventory.AddToCartAsync(last);
            await Expect(inventory.Header.CartBadge).ToHaveTextAsync("2");
            return last;
        });

        var overview = await Step("6. Go to checkout", async () =>
        {
            cart = await inventory.Header.OpenCartAsync();
            await Expect(cart.Items).ToHaveCountAsync(2);
            Assert.That(await cart.GetItemNamesAsync(), Is.EquivalentTo(new[] { lowest.Name, lastByName }));

            var information = await cart.CheckoutAsync();
            return await information.ContinueAsync(customer);
        });

        var complete = await Step("7. Check the order summary and finish the order", async () =>
        {
            await Expect(overview.Items).ToHaveCountAsync(2);
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
            return await overview.FinishAsync();
        });

        await Step("8. Verify the order is placed", async () =>
        {
            await Expect(complete.ConfirmationHeader).ToHaveTextAsync("Thank you for your order!");
            await Expect(Page).ToHaveURLAsync(new Regex("checkout-complete"));
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
