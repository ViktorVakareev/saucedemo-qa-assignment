using SauceDemo.UiTests.Config;
using SauceDemo.UiTests.Infrastructure;
using SauceDemo.UiTests.Pages;

namespace SauceDemo.UiTests.Tests;

[TestFixture]
[Category("UI")]
[Parallelizable(ParallelScope.Self)]
public sealed class CheckoutTotalsTests : UiTestBase
{
    [Test]
    [Category("KnownDefect")]
    [Description("BUG-001 in BugReport.md: Item total shows floating-point noise ($59.980000000000004). Fails until fixed.")]
    public async Task Item_total_is_shown_as_a_two_decimal_amount()
    {
        var login = await new LoginPage(Page).OpenAsync();
        var inventory = await login.LoginAsync(await CredentialsProvider.ResolveAsync(login, Settings));
        await inventory.AddToCartAsync("Sauce Labs Bike Light");     // $9.99
        await inventory.AddToCartAsync("Sauce Labs Fleece Jacket");  // $49.99

        var cart = await inventory.Header.OpenCartAsync();
        await Expect(cart.Items).ToHaveCountAsync(2);
        var overview = await (await cart.CheckoutAsync()).ContinueAsync(CheckoutCustomer.Load());

        await Expect(overview.ItemTotalLabel).ToHaveTextAsync("Item total: $59.98");
    }
}
