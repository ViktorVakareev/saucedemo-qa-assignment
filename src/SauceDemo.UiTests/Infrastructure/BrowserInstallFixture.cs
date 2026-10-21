// Root namespace on purpose: an NUnit [SetUpFixture] only applies to tests in its namespace and below.
namespace SauceDemo.UiTests;

/// <summary>
/// Installs the Playwright browser once per run, so a fresh clone works with plain `dotnet test`
/// (no separate playwright.ps1 step). Fast no-op when the browser is already there.
/// Set PLAYWRIGHT_SKIP_INSTALL=1 when the pipeline installs browsers itself.
/// </summary>
[SetUpFixture]
public sealed class BrowserInstallFixture
{
    [OneTimeSetUp]
    public void InstallBrowser()
    {
        if (Environment.GetEnvironmentVariable("PLAYWRIGHT_SKIP_INSTALL") == "1") return;

        var browser = Environment.GetEnvironmentVariable("BROWSER") is { Length: > 0 } b ? b : "chromium";
        var exitCode = Microsoft.Playwright.Program.Main(["install", browser]);
        if (exitCode != 0)
            throw new InvalidOperationException($"Playwright browser install for '{browser}' failed with exit code {exitCode}.");
    }
}
