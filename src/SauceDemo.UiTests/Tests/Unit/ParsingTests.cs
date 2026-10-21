using SauceDemo.UiTests.Config;
using SauceDemo.UiTests.Models;

namespace SauceDemo.UiTests.Tests.Unit;

/// <summary>Fast checks of the parsing logic the UI test depends on; no browser.</summary>
[TestFixture]
[Category("Unit")]
public sealed class ParsingTests
{
    private const string UserHint = "Accepted usernames are:\nstandard_user\nlocked_out_user\nproblem_user\nperformance_glitch_user\n";
    private const string PasswordHint = "Password for all users:\nsecret_sauce";

    [Test]
    public void Reads_user_names_without_the_heading() =>
        Assert.That(CredentialsParser.ParseUserNames(UserHint),
            Is.EqualTo(new[] { "standard_user", "locked_out_user", "problem_user", "performance_glitch_user" }));

    [Test]
    public void Reads_the_password_without_the_heading() =>
        Assert.That(CredentialsParser.ParsePassword(PasswordHint), Is.EqualTo("secret_sauce"));

    [Test]
    public void Handles_windows_line_endings_and_spaces() =>
        Assert.That(CredentialsParser.ParsePassword("Password for all users:\r\n  secret_sauce  \r\n"), Is.EqualTo("secret_sauce"));

    [Test]
    public void Picks_the_configured_user() =>
        Assert.That(CredentialsParser.PickUser(["locked_out_user", "standard_user"], "standard_user"), Is.EqualTo("standard_user"));

    [Test]
    public void Falls_back_to_a_renamed_standard_user() =>
        Assert.That(CredentialsParser.PickUser(["standard_user_v2", "locked_out_user"], "standard_user"), Is.EqualTo("standard_user_v2"));

    [Test]
    public void Fails_clearly_when_the_user_is_missing() =>
        Assert.That(() => CredentialsParser.PickUser(["locked_out_user"], "standard_user"),
            Throws.InvalidOperationException.With.Message.Contains("SAUCEDEMO_USERNAME"));

    [Test]
    public void Credentials_never_print_the_password() =>
        Assert.That(new Credentials("standard_user", "secret_sauce").ToString(), Does.Not.Contain("secret_sauce"));

    [TestCase("$29.99", 29.99)]
    [TestCase("$7.99", 7.99)]
    [TestCase("Item total: $37.98", 37.98)]
    [TestCase("Tax: $3.04", 3.04)]
    [TestCase("Total: $41.02", 41.02)]
    public void Parses_money(string text, decimal expected) =>
        Assert.That(Money.Parse(text), Is.EqualTo(expected));

    [Test]
    public void Rejects_text_without_an_amount() =>
        Assert.That(() => Money.Parse("free"), Throws.TypeOf<FormatException>());
}
