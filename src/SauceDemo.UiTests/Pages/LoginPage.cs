using SauceDemo.UiTests.Config;

namespace SauceDemo.UiTests.Pages;

public sealed class LoginPage(IPage page)
{
    private ILocator UserNamesHint => page.Locator("[data-test='login-credentials']");
    private ILocator PasswordHint => page.Locator("[data-test='login-password']");
    private ILocator DismissErrorButton => page.Locator("[data-test='error-button']");

    public ILocator UserNameInput => page.Locator("[data-test='username']");
    public ILocator PasswordInput => page.Locator("[data-test='password']");
    public ILocator LoginButton => page.Locator("[data-test='login-button']");
    public ILocator Error => page.Locator("[data-test='error']");

    public async Task<LoginPage> OpenAsync()
    {
        await page.GotoAsync("/");
        return this;
    }

    public Task<string> ReadUserNamesHintAsync() => UserNamesHint.InnerTextAsync();
    public Task<string> ReadPasswordHintAsync() => PasswordHint.InnerTextAsync();

    /// <summary>Fills the form and submits; empty strings leave a field empty.</summary>
    public async Task SubmitAsync(string userName, string password)
    {
        await UserNameInput.FillAsync(userName);
        await PasswordInput.FillAsync(password);
        await LoginButton.ClickAsync();
    }

    public async Task<InventoryPage> LoginAsync(Credentials credentials)
    {
        await SubmitAsync(credentials.UserName, credentials.Password);
        return new InventoryPage(page);
    }

    public Task DismissErrorAsync() => DismissErrorButton.ClickAsync();
}
