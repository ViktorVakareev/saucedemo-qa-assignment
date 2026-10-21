using System.Text.Json;

namespace SauceDemo.UiTests.Config;

/// <summary>
/// Settings from appsettings.json, overridable by environment variables (CI, other environments).
/// </summary>
public sealed record TestSettings
{
    public string BaseUrl { get; init; } = "https://www.saucedemo.com";
    public string UserName { get; init; } = "standard_user";
    public int DefaultTimeoutMs { get; init; } = 10_000;
    public int ExpectTimeoutMs { get; init; } = 5_000;

    private static readonly Lazy<TestSettings> Loaded = new(Load);

    public static TestSettings Current => Loaded.Value;

    private static TestSettings Load()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        var settings = File.Exists(path)
            ? JsonSerializer.Deserialize<TestSettings>(File.ReadAllText(path)) ?? new TestSettings()
            : new TestSettings();

        return settings with
        {
            BaseUrl = Env("SAUCEDEMO_BASE_URL") ?? settings.BaseUrl,
        };
    }

    internal static string? Env(string name) =>
        Environment.GetEnvironmentVariable(name) is { Length: > 0 } value ? value : null;
}
