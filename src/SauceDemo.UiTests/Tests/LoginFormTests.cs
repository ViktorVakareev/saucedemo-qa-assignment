using System.Text.RegularExpressions;
using SauceDemo.UiTests.Config;
using SauceDemo.UiTests.Infrastructure;
using SauceDemo.UiTests.Pages;

namespace SauceDemo.UiTests.Tests;

/// <summary>Login form validation: required fields, wrong credentials, locked-out user, error handling.</summary>
[TestFixture]
[Category("UI")]
[Parallelizable(ParallelScope.Self)]
public sealed partial class LoginFormTests : UiTestBase
{
    private LoginPage _login = null!;
    private Credentials _valid = null!;
    private string _lockedOutUser = null!;

    [SetUp]
    public async Task OpenLoginPageAsync()
    {
        _login = await new LoginPage(Page).OpenAsync();
        _valid = await CredentialsProvider.ResolveAsync(_login, Settings);
        var listedUsers = CredentialsParser.ParseUserNames(await _login.ReadUserNamesHintAsync());
        _lockedOutUser = CredentialsParser.PickUser(listedUsers, "locked_out_user");
    }

    // Which values to type is decided per case from the resolved credentials, so nothing is hard-coded.
    public enum Input { Empty, ValidUser, ValidPassword, WrongPassword, LockedOutUser }

    private static readonly object[] InvalidLogins =
    [
        new object[] { Input.Empty, Input.Empty, "Epic sadface: Username is required" },
        new object[] { Input.ValidUser, Input.Empty, "Epic sadface: Password is required" },
        new object[] { Input.Empty, Input.ValidPassword, "Epic sadface: Username is required" },
        new object[] { Input.ValidUser, Input.WrongPassword, "Epic sadface: Username and password do not match any user in this service" },
        new object[] { Input.LockedOutUser, Input.ValidPassword, "Epic sadface: Sorry, this user has been locked out." },
    ];

    [TestCaseSource(nameof(InvalidLogins))]
    public async Task Invalid_login_is_rejected_with_a_clear_message(Input user, Input password, string expectedError)
    {
        await _login.SubmitAsync(Value(user), Value(password));

        await Expect(_login.Error).ToHaveTextAsync(expectedError);
        await Expect(Page).Not.ToHaveURLAsync(InventoryUrl());
    }

    [Test]
    public async Task Error_message_can_be_dismissed()
    {
        await _login.SubmitAsync("", "");
        await Expect(_login.Error).ToBeVisibleAsync();

        await _login.DismissErrorAsync();

        await Expect(_login.Error).ToHaveCountAsync(0);
        await Expect(_login.UserNameInput).Not.ToHaveClassAsync(ErrorClass());
    }

    [Test]
    public async Task Password_field_is_masked() =>
        await Expect(_login.PasswordInput).ToHaveAttributeAsync("type", "password");

    [Test]
    [Category("KnownDefect")]
    [Description("Observation O-1 in BugReport.md: the valid username field is also marked as invalid. Fails until fixed.")]
    public async Task Only_the_missing_field_is_marked_as_invalid()
    {
        await _login.SubmitAsync(_valid.UserName, "");

        await Expect(_login.Error).ToHaveTextAsync("Epic sadface: Password is required");
        await Expect(_login.PasswordInput).ToHaveClassAsync(ErrorClass());
        await Expect(_login.UserNameInput).Not.ToHaveClassAsync(ErrorClass());
    }

    private string Value(Input input) => input switch
    {
        Input.Empty => "",
        Input.ValidUser => _valid.UserName,
        Input.ValidPassword => _valid.Password,
        Input.WrongPassword => _valid.Password + "-wrong",
        Input.LockedOutUser => _lockedOutUser,
        _ => throw new ArgumentOutOfRangeException(nameof(input)),
    };

    [GeneratedRegex(@"\berror\b")]
    private static partial Regex ErrorClass();

    [GeneratedRegex("inventory")]
    private static partial Regex InventoryUrl();
}
