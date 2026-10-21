# With more time

I kept the solution to the 3–4 hour timebox. This is what I'd add next, in order of value.

| # | Improvement | Why |
|---|---|---|
| 1 | Automate the money rule in the UI: the price summary for every product pair, in both add orders | `PriceSummaryArithmeticTests` replays the arithmetic offline; a UI version proves what the customer actually sees (BUG-001) |
| 2 | Automate the rest of `AdditionalTestCases.feature`: blank and international customer details, markup as text, Back after logout, Reset App State | Page objects for all of it already exist |
| 3 | Run the scenario for the other listed users (`problem_user`, `performance_glitch_user`, `error_user`, `visual_user`) | Each simulates a class of defect: broken UI, slow responses, JS errors, visual changes. Expected failures become documented known defects |
| 4 | Cross-browser matrix in CI (Chromium, Firefox, WebKit) | One env variable already switches the browser; CI would run the three as parallel jobs |
| 5 | An HTML report (Allure or similar) published from CI | Steps, screenshots and traces in one place for people who don't read TRX |
| 6 | Accessibility checks with axe-core (Deque's `Deque.AxeCore.Playwright`) on login, cart and checkout | O-1 is partly an accessibility issue; automated checks would find similar ones |
| 7 | Visual comparison of key pages (Playwright for .NET has no built-in screenshot assertion; e.g. Verify or a visual-testing service) | Layout defects like `visual_user`'s aren't caught by DOM assertions |
| 8 | API-based setup: log in and fill the cart through storage state instead of the UI | Faster tests that only click through the part they're testing |
| 9 | Performance budget for login and checkout (navigation timings, `performance_glitch_user`) | Turns "feels slow" into a threshold that fails the build |
| 10 | Bind the Gherkin scenarios worth automating to Reqnroll | The scenarios map directly onto the existing page objects |
| 11 | Static checks in CI: `dotnet format --verify-no-changes`, analyzers, Dependabot | Keeps the code consistent and dependencies current |
