using System.Globalization;
using System.Text.RegularExpressions;

namespace SauceDemo.UiTests.Models;

public sealed record Product(string Name, decimal Price);

public static partial class Money
{
    /// <summary>Parses "$29.99" or "Item total: $29.99" into 29.99.</summary>
    public static decimal Parse(string text)
    {
        var match = AmountPattern().Match(text);
        return match.Success
            ? decimal.Parse(match.Groups[1].Value, NumberStyles.Number, CultureInfo.InvariantCulture)
            : throw new FormatException($"No amount found in '{text}'.");
    }

    [GeneratedRegex(@"\$\s*(\d+(?:\.\d+)?)")]
    private static partial Regex AmountPattern();
}
