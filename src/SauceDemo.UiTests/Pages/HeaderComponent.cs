namespace SauceDemo.UiTests.Pages;

/// <summary>Cart link, cart badge and the burger menu: shared by every page after login.</summary>
public sealed class HeaderComponent(IPage page)
{
    public ILocator CartBadge => page.Locator("[data-test='shopping-cart-badge']");
    private ILocator CartLink => page.Locator("[data-test='shopping-cart-link']");
    private ILocator MenuButton => page.Locator("#react-burger-menu-btn");
    private ILocator LogoutLink => page.Locator("[data-test='logout-sidebar-link']");

    public async Task<CartPage> OpenCartAsync()
    {
        await CartLink.ClickAsync();
        return new CartPage(page);
    }

    public async Task<LoginPage> LogoutAsync()
    {
        await MenuButton.ClickAsync();
        await LogoutLink.ClickAsync();
        return new LoginPage(page);
    }
}
