# SauceDemo QA assignment

[![UI tests](https://github.com/ViktorVakareev/saucedemo-qa-assignment/actions/workflows/ui-tests.yml/badge.svg)](https://github.com/ViktorVakareev/saucedemo-qa-assignment/actions/workflows/ui-tests.yml)

Manual test design, exploratory testing and Playwright UI automation in C# for [SauceDemo](https://www.saucedemo.com/).

| Part | Deliverable |
|---|---|
| 1. Manual testing | [`ManualTestCases.feature`](ManualTestCases.feature): the required positive, two negative and one edge-case scenario |
| 2. Exploratory testing | [`BugReport.md`](BugReport.md): BUG-001 in full, plus observations O-1 to O-4 |
| 3. UI automation | [`src/SauceDemo.UiTests`](src/SauceDemo.UiTests): Playwright for .NET + NUnit |
| Beyond the brief | [`AdditionalTestCases.feature`](AdditionalTestCases.feature): more scenarios chosen from the findings · [`IMPROVEMENTS.md`](IMPROVEMENTS.md): what I'd add next |

**Testing priorities.** I tested this shop the way I'd test a financial application: amounts must be exact, protected pages must need a session, one user's data must not reach another, and every finding must come with evidence. That's why the bug report leads with a money defect, and why the extra tests cover access control and cart isolation.

## Contents

- [Run it](#run-it)
- [Credentials: "prone to change"](#credentials-prone-to-change)
- [The automated scenario](#the-automated-scenario)
- [All tests](#all-tests)
- [Design](#design)
- [CI](#ci)
- [Assumptions](#assumptions)

## Run it

Needs the [.NET 10 SDK](https://dotnet.microsoft.com/download). Nothing else: the first run installs the Playwright browser.

```powershell
git clone https://github.com/ViktorVakareev/saucedemo-qa-assignment.git
cd saucedemo-qa-assignment
dotnet test --filter "TestCategory!=KnownDefect"      # what the CI gate runs
```

| Command | What it does |
|---|---|
| `dotnet test --filter "FullyQualifiedName~PurchaseFlowTests"` | Only the assignment scenario |
| `dotnet test --filter "TestCategory=Unit"` | Unit tests only, no browser |
| `dotnet test --filter "TestCategory=KnownDefect"` | Tests that reproduce the reported defects (expected to fail) |
| `$env:HEADED="1"; dotnet test ...` | Watch the browser |
| `$env:BROWSER="firefox"; dotnet test ...` | Another browser: `chromium`, `firefox` or `webkit` |

![Local test run](docs/test-run-local.png)

When a UI test fails, a Playwright trace is saved under `src/SauceDemo.UiTests/bin/<config>/net10.0/playwright-traces/` and attached to the test result. Open it by dropping the `.zip` on [trace.playwright.dev](https://trace.playwright.dev); it runs in the browser and the file isn't uploaded.

## Credentials: "prone to change"

No user name or password is stored in the repository. `CredentialsProvider` resolves them at runtime:

1. **Environment variables** `SAUCEDEMO_USERNAME` and `SAUCEDEMO_PASSWORD`, if both are set (CI secrets, another environment).
2. Otherwise **the hints SauceDemo publishes on its login page** ("Accepted usernames are: …", "Password for all users: …"), read before logging in. The wanted user (`standard_user`, from `appsettings.json`) is picked from that list. If it was renamed, the only listed user with the same prefix is used. If it's missing, the test fails with a message listing the users that are there.

A changed password or user name needs no code change. The parsing is unit-tested, and `Credentials.ToString()` never prints the password.

## The automated scenario

`PurchaseFlowTests.Standard_user_completes_an_order_after_changing_the_cart`, one named step per assignment step:

| Step | What's checked |
|---|---|
| 1–2. Log in | Credentials resolved as above; "Products" page shown |
| 3. Highest and lowest price | Found **from the prices on the page**, not hard-coded; badge 2; cart lists exactly those two |
| 4. Remove the highest | Cart lists only the lowest; badge 1 |
| 5. Sort Name (A to Z), add the last | The list really is in A–Z order; the last product is added; badge 2 |
| 6. Checkout | Cart lists the lowest + last product; customer data from `TestData/checkout-customer.json` |
| 7. Finish | Overview lists the same two; item total = sum of prices; total = item total + tax |
| 8. Order placed | "Thank you for your order!" and the `checkout-complete` URL |
| 9. Cart empty | No badge; the cart page has no items |
| 10. Log out | Login form shown; `/inventory.html` is refused afterwards, so the session really ended |

## All tests

| Class | Tests | What it covers |
|---|---|---|
| `PurchaseFlowTests` | 1 | The assignment scenario |
| `LoginFormTests` | 7 + 1 known defect | Required fields, wrong password, locked-out user, dismissing the error, masked password; O-1 |
| `SessionAndCartTests` | 6 + 2 known defects | Every protected page redirects to login without a session; O-2 (empty-cart checkout), O-4 (cart shared between users) |
| `CheckoutTotalsTests` | 1 known defect | BUG-001 |
| `Unit/ParsingTests` | 13 | Credential and money parsing |
| `Unit/PriceSummaryArithmeticTests` | 3 | Replays the site's price arithmetic for all 1,956 carts; backs the tax and add-order analysis in BUG-001 |

**Gate: 30 tests. Known defects: 4** (they describe correct behaviour, so they fail until the defects are fixed).

## Design

```
src/SauceDemo.UiTests/
  Config/          settings (appsettings.json + env overrides), credentials provider and parser, test-data loader
  Pages/           page objects: Login, Inventory, Cart, Checkout (3 steps), Header (cart + menu)
  Models/          Product, money parsing
  Infrastructure/  UiTestBase (context options, timeouts, trace on failure, steps), browser install fixture
  Tests/           UI tests, plus Unit/ for logic that needs no browser
  TestData/        checkout-customer.json
```

- **Page objects return the next page** (`cart.CheckoutAsync()` returns the information page), so a test reads like the user journey.
- **Locators** use `data-test` attributes and accessible roles ("Add to cart", "Remove"), and live only in page objects. Products are found by exact name, never by position.
- **No sleeps.** Playwright's web-first assertions (`ToHaveTextAsync`, `ToHaveCountAsync`) wait for the UI. Plain reads like `AllInnerTextsAsync()` don't wait, so each read is preceded by an assertion that the content is there. The first CI run caught one place where it wasn't.
- **Each step hands its result to the next** (`Step<T>`): the page it ended on, the products it picked. No shared mutable state.
- **Isolation:** every test gets a new browser context, so no cart or login state is shared, and tests can run in parallel.
- **Configuration, test data and secrets are kept apart from code.**
- **Known defects are tests too** (`KnownDefect` category): they document the defect and turn green when it's fixed, without blocking the pipeline meanwhile.

## CI

[`.github/workflows/ui-tests.yml`](.github/workflows/ui-tests.yml) runs on every push and pull request:

| Job | Does | Blocks? |
|---|---|---|
| `test` | Build, install Chromium, run everything except known defects, upload TRX results and failure traces | Yes |
| `known-defects` | Run the known-defect tests, upload their results and traces as `known-defect-evidence` | No: `continue-on-error` on the test step, so the job is green and the step shows the expected failures |

The runner image (`ubuntu-24.04`) and the major versions of the actions are fixed, so the pipeline changes only when we change it. Trace files stay out of git: they're regenerated by every run and record everything typed into the page. A dated log of the known-defect run is kept in [`docs/evidence/`](docs/evidence/).

## Assumptions

- Step 3 treats "highest" and "lowest" as unique prices (true for the current catalogue; the test asserts they differ).
- Step 5 assumes the last product A–Z isn't already in the cart; the test asserts it, so a catalogue change fails clearly.
- Expected results with no specification on the site (for example an empty cart) are marked as assumptions in the feature files.
- The Gherkin scenarios are test design, as the assignment asks; they aren't bound to Reqnroll.
