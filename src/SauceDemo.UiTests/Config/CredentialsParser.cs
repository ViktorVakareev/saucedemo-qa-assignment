namespace SauceDemo.UiTests.Config;

/// <summary>
/// Parses the credential hints SauceDemo shows on its login page, for example:
/// "Accepted usernames are:\nstandard_user\nlocked_out_user ..." and "Password for all users:\nsecret_sauce".
/// Pure functions, so they're unit-tested without a browser.
/// </summary>
public static class CredentialsParser
{
    public static IReadOnlyList<string> ParseUserNames(string panelText) =>
        Values(panelText).ToList();

    public static string ParsePassword(string panelText) =>
        Values(panelText).FirstOrDefault()
        ?? throw new InvalidOperationException($"No password found in the login page hint: '{panelText}'.");

    /// <summary>Picks the wanted user; if it was renamed, falls back to the only name with the same prefix (e.g. "standard").</summary>
    public static string PickUser(IReadOnlyList<string> available, string wanted)
    {
        if (available.Contains(wanted)) return wanted;

        var prefix = wanted.Split('_')[0];
        var candidates = available.Where(u => u.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)).ToList();
        return candidates.Count == 1
            ? candidates[0]
            : throw new InvalidOperationException(
                $"User '{wanted}' isn't listed on the login page. Listed: {string.Join(", ", available)}. " +
                "Set SAUCEDEMO_USERNAME / SAUCEDEMO_PASSWORD or update UserName in appsettings.json.");
    }

    // Lines that are values, not headings like "Accepted usernames are:".
    private static IEnumerable<string> Values(string text) =>
        text.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(line => !line.EndsWith(':'));
}
