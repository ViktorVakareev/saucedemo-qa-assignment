# With more time

I kept the solution to the 3–4 hour timebox. This is what I'd add next, roughly in order of value.

| # | Improvement | Why |
|---|---|---|
| 1 | Data-driven price-summary test over every pair of catalogue products | Catches BUG-001-type formatting and rounding errors anywhere, not just in the pair I found |
| 2 | Run the scenario for the other listed users (`problem_user`, `performance_glitch_user`, `error_user`, `visual_user`) | Each simulates a class of defect: broken UI, slow responses, JS errors, visual changes. Expected failures become documented known defects |
| 3 | Cross-browser matrix in CI (Chromium, Firefox, WebKit) | One env variable already switches the browser; CI would run the three in parallel jobs |
| 4 | Reporting: Allure or the Playwright HTML-like report, published from CI | Steps, screenshots and traces in one place for non-developers |
| 5 | Visual regression on key pages (`ToHaveScreenshotAsync` equivalent or a visual service) | Layout bugs like `visual_user`'s aren't caught by DOM assertions |
| 6 | Accessibility checks (axe-core via Deque's .NET package) on login, cart and checkout | O-1 in the bug report is partly an accessibility issue; automated checks would catch similar ones |
| 7 | Bind `ManualTestCases.feature` to Reqnroll for the scenarios worth automating | The positive and negative checkout scenarios map directly onto the existing page objects |
| 8 | Performance budget for login and checkout (navigation timings, `performance_glitch_user`) | Turns "feels slow" into a threshold that fails the build |
| 9 | Split the page objects into a reusable framework project, versioned separately | Worth it once several suites or products share it |
| 10 | Test-data builder for customers (valid, boundary, unicode, very long values) | Feeds negative and boundary tests for the checkout form |
| 11 | Static checks in CI: `dotnet format --verify-no-changes`, analyzers, Dependabot | Keeps the code base consistent and dependencies current |
