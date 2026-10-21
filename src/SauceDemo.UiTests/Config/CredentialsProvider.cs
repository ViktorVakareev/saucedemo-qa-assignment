using SauceDemo.UiTests.Pages;

namespace SauceDemo.UiTests.Config;

/// <summary>
/// Resolves credentials without hard-coding them:
/// 1. SAUCEDEMO_USERNAME + SAUCEDEMO_PASSWORD environment variables (CI secrets, other environments);
/// 2. otherwise the hints published on the SauceDemo login page, read at runtime.
/// </summary>
public static class CredentialsProvider
{
    public static async Task<Credentials> ResolveAsync(LoginPage loginPage, TestSettings settings)
    {
        var envUser = TestSettings.Env("SAUCEDEMO_USERNAME");
        var envPassword = TestSettings.Env("SAUCEDEMO_PASSWORD");
        if (envUser is not null && envPassword is not null)
            return new Credentials(envUser, envPassword);

        var users = CredentialsParser.ParseUserNames(await loginPage.ReadUserNamesHintAsync());
        var password = CredentialsParser.ParsePassword(await loginPage.ReadPasswordHintAsync());
        return new Credentials(CredentialsParser.PickUser(users, settings.UserName), password);
    }
}
