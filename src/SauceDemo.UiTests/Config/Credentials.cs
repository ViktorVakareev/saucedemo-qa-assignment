namespace SauceDemo.UiTests.Config;

public sealed record Credentials(string UserName, string Password)
{
    // Never print the password in logs or reports.
    public override string ToString() => $"{UserName} / ********";
}
