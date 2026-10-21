using Microsoft.Playwright.NUnit;
using NUnit.Framework.Interfaces;
using SauceDemo.UiTests.Config;

namespace SauceDemo.UiTests.Infrastructure;

/// <summary>
/// Base for UI tests: a fresh browser context per test (isolated cookies and local storage),
/// base URL and timeouts from settings, and a Playwright trace kept only when a test fails.
/// </summary>
public abstract class UiTestBase : PageTest
{
    protected static TestSettings Settings => TestSettings.Current;

    public override BrowserNewContextOptions ContextOptions() => new()
    {
        BaseURL = Settings.BaseUrl,
        ViewportSize = new() { Width = 1366, Height = 900 },
    };

    [SetUp]
    public async Task StartTracingAsync()
    {
        Page.SetDefaultTimeout(Settings.DefaultTimeoutMs);
        Assertions.SetDefaultExpectTimeout(Settings.ExpectTimeoutMs);

        await Context.Tracing.StartAsync(new()
        {
            Title = TestContext.CurrentContext.Test.Name,
            Screenshots = true,
            Snapshots = true,
            Sources = true,
        });
    }

    [TearDown]
    public async Task StopTracingAsync()
    {
        var failed = TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed;
        string? path = null;
        if (failed)
        {
            var dir = Path.Combine(TestContext.CurrentContext.WorkDirectory, "playwright-traces");
            Directory.CreateDirectory(dir);
            path = Path.Combine(dir, $"{SafeFileName(TestContext.CurrentContext.Test.Name)}-{DateTime.UtcNow:yyyyMMdd-HHmmss}.zip");
        }

        await Context.Tracing.StopAsync(new() { Path = path });   // null path = discard the trace
        if (path is not null)
            TestContext.AddTestAttachment(path, "Playwright trace: npx playwright show-trace <file> or trace.playwright.dev");
    }

    /// <summary>Named step in the test log, so a failure says which business step broke.</summary>
    protected static async Task Step(string name, Func<Task> action)
    {
        TestContext.Progress.WriteLine($"STEP: {name}");
        await action();
    }

    /// <summary>A named step that hands its result to the next step (the page it ended on, the products it picked).</summary>
    protected static async Task<T> Step<T>(string name, Func<Task<T>> action)
    {
        TestContext.Progress.WriteLine($"STEP: {name}");
        return await action();
    }

    // Parameterised test names contain quotes, colons and commas, which Windows doesn't allow in file names.
    private static string SafeFileName(string testName)
    {
        var invalid = Path.GetInvalidFileNameChars().Concat(new[] { '"', ':', ',', '(', ')', ' ' }).ToHashSet();
        var safe = new string(testName.Select(c => invalid.Contains(c) ? '_' : c).ToArray());
        return safe.Length <= 100 ? safe : safe[..100];
    }
}
