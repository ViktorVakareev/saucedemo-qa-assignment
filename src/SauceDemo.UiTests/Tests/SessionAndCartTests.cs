using SauceDemo.UiTests.Config;
using SauceDemo.UiTests.Infrastructure;
using SauceDemo.UiTests.Pages;

namespace SauceDemo.UiTests.Tests;

/// <summary>
/// Beyond the assignment scenario: access control and cart state across sessions,
/// the areas a financial application is judged on first. See AdditionalTestCases.feature.
/// </summary>
[TestFixture]
[Category("UI")]
[Parallelizable(ParallelScope.Self)]
public sealed class SessionAndCartTests : UiTestBase
{
    [TestCase("/inventory.html")]
    [TestCase("/inventory-item.html?id=4")]
    [TestCase("/cart.html")]
    [TestCase("/checkout-step-one.html")]
    [TestCase("/checkout-step-two.html")]
    [TestCase("/checkout-complete.html")]
    public async Task Protected_page_redirects_to_login_without_a_session(string path)
    {
        await Page.GotoAsync(path);

        var login = new LoginPage(Page);
        await Expect(login.LoginButton).ToBeVisibleAsync();
        await Expect(login.Error).ToHaveTextAsync($"Epic sadface: You can only access '{path.Split('?')[0]}' when you are logged in.");
    }

    [Test]
    [Category("KnownDefect")]
    [Description("O-2 in BugReport.md: Checkout is offered with an empty cart. Fails until fixed.")]
    public async Task Checkout_is_not_offered_for_an_empty_cart()
    {
        var inventory = await LogInAsync(Settings.UserName);

        var cart = await inventory.Header.OpenCartAsync();
        await Expect(cart.Items).ToHaveCountAsync(0);

        await Expect(cart.CheckoutButton).ToBeDisabledAsync();
    }

    [Test]
    [Category("KnownDefect")]
    [Description("O-4 in BugReport.md: the cart is stored per browser, not per user, and logout keeps it. Fails until fixed.")]
    public async Task Next_user_on_the_same_browser_does_not_see_the_previous_users_cart()
    {
        var first = await LogInAsync(Settings.UserName);
        await first.AddToCartAsync("Sauce Labs Backpack");
        await Expect(first.Header.CartBadge).ToHaveTextAsync("1");
        await first.Header.LogoutAsync();

        var second = await LogInAsync("visual_user");

        await Expect(second.Header.CartBadge).ToHaveCountAsync(0);
    }

    /// <summary>Logs in as a user listed on the login page (same credential rules as the main scenario).</summary>
    private async Task<InventoryPage> LogInAsync(string wantedUser)
    {
        var login = await new LoginPage(Page).OpenAsync();
        var resolved = await CredentialsProvider.ResolveAsync(login, Settings with { UserName = wantedUser });
        var inventory = await login.LoginAsync(resolved);
        await Expect(inventory.Title).ToHaveTextAsync("Products");
        return inventory;
    }
}
