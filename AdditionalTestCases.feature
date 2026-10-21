# Additional test cases, beyond the four the assignment asks for (see ManualTestCases.feature).
# Chosen from what exploratory testing found, with the priorities of a financial application:
# money must be exact, access must be controlled, one user's data must never reach another, input must be validated.
#
# @automated = also covered by an automated test (class and method named in the scenario's comment).
# @known-defect = describes the correct behaviour and currently fails; see BugReport.md.
# Expected results marked "assumption" have no specification on the site and would be confirmed with the product owner.

Feature: Additional checks for SauceDemo

  Background:
    Given I open the SauceDemo login page in a new browser session

  # ================================================================ money
  Rule: Amounts are shown and calculated exactly

    @money @known-defect @automated
    Scenario Outline: The price summary shows two decimals for any cart
      # CheckoutTotalsTests.Item_total_is_shown_as_a_two_decimal_amount (first row); BUG-001
      Given I am logged in as "standard_user"
      When I add "<first product>" and then "<second product>" to the cart
      And I go to "Checkout: Overview" with valid customer details
      Then "Item total" is "<item total>"
      And "Tax" is "<tax>"
      And "Total" is "<total>"

      Examples:
        | first product         | second product           | item total | tax   | total  |
        | Sauce Labs Bike Light | Sauce Labs Fleece Jacket | $59.98     | $4.80 | $64.78 |
        | Sauce Labs Onesie     | Sauce Labs Fleece Jacket | $57.98     | $4.64 | $62.62 |
        | Sauce Labs Backpack   | Sauce Labs Onesie        | $37.98     | $3.04 | $41.02 |

    @money @known-defect
    Scenario: The totals don't depend on the order products were added
      # Fails today in the first order: "Item total: $105.96000000000001" (BUG-001)
      Given I am logged in as "standard_user"
      When I add "Sauce Labs Backpack", "Sauce Labs Bike Light", "Sauce Labs Bolt T-Shirt" and "Sauce Labs Fleece Jacket" in that order
      And I note the price summary on "Checkout: Overview"
      And I empty the cart and add the same four products in reverse order
      Then the price summary on "Checkout: Overview" is identical
      And "Item total" is "$105.96" both times

    @money
    Scenario: Tax is 8% of the item total, rounded to cents (assumption)
      # Rate inferred from the observed totals; PriceSummaryArithmeticTests checks the rule for all 1,956 carts
      Given I am logged in as "standard_user"
      When I add "Sauce Labs Backpack" to the cart
      And I go to "Checkout: Overview" with valid customer details
      Then "Tax" is "$2.40"
      And "Total" is "Item total" + "Tax"

  # ================================================================ access and sessions
  Rule: Protected pages and data need a valid session

    @security @automated
    Scenario Outline: A protected page sends a visitor without a session to the login page
      # SessionAndCartTests.Protected_page_redirects_to_login_without_a_session
      When I open "<page>" directly
      Then I am on the login page
      And I see the error "Epic sadface: You can only access '<page>' when you are logged in."

      Examples:
        | page                    |
        | /inventory.html         |
        | /cart.html              |
        | /checkout-step-one.html |
        | /checkout-step-two.html |
        | /checkout-complete.html |

    @security @known-defect @automated
    Scenario: The next user on the same browser doesn't see the previous user's cart
      # SessionAndCartTests.Next_user_on_the_same_browser_does_not_see_the_previous_users_cart; O-4
      Given I am logged in as "standard_user"
      And I add "Sauce Labs Backpack" to the cart
      When I log out
      And I log in as "visual_user"
      Then the cart badge is not shown
      And the cart is empty

    @security
    Scenario: The browser Back button after logout doesn't show protected pages
      Given I am logged in as "standard_user" on the "Products" page
      When I log out
      And I press the browser's Back button
      Then I am on the login page
      And no product or cart data is shown

  # ================================================================ cart
  Rule: The cart reflects exactly what the shopper chose

    @cart @known-defect @automated
    Scenario: Checkout isn't offered for an empty cart (assumption)
      # SessionAndCartTests.Checkout_is_not_offered_for_an_empty_cart; O-2
      Given I am logged in as "standard_user"
      And my cart is empty
      When I open the cart
      Then the "Checkout" button is disabled

    @cart
    Scenario: Removing a product from the product list updates the cart
      Given I am logged in as "standard_user"
      And I add "Sauce Labs Backpack" and "Sauce Labs Bike Light" to the cart
      When I click "Remove" for "Sauce Labs Backpack" on the "Products" page
      Then the cart badge shows "1"
      And the cart lists only "Sauce Labs Bike Light"

    @cart
    Scenario: "Reset App State" empties the cart and resets every product button
      Given I am logged in as "standard_user"
      And I add "Sauce Labs Backpack" to the cart
      When I open the menu and click "Reset App State"
      Then the cart badge is not shown
      And the button for "Sauce Labs Backpack" shows "Add to cart"

  # ================================================================ input validation
  Rule: Customer details are validated before the order continues

    @validation
    Scenario Outline: Blank customer details are rejected (assumption)
      Given I am logged in as "standard_user" with "Sauce Labs Bike Light" in the cart
      And I am on the "Checkout: Your Information" page
      When I enter first name "<first name>", last name "<last name>" and postal code "<postal code>"
      And I click "Continue"
      Then I stay on the "Checkout: Your Information" page
      And I see an error for the blank field

      # "␣" stands for spaces only
      Examples:
        | first name | last name | postal code |
        | ␣␣␣        | Lovelace  | 1000        |
        | Ada        | ␣␣␣       | 1000        |
        | Ada        | Lovelace  | ␣␣␣         |

    @validation
    Scenario Outline: Valid international and edge-length details are accepted and shown unchanged
      Given I am logged in as "standard_user" with "Sauce Labs Bike Light" in the cart
      When I enter first name "<first name>", last name "<last name>" and postal code "<postal code>"
      And I click "Continue"
      Then I reach "Checkout: Overview"

      Examples:
        | first name | last name     | postal code |
        | Zoë        | Ñúñez-O'Brien | SW1A 1AA    |
        | 李         | 王            | 100-0001    |
        | A          | B             | 1           |
        | A × 100    | B × 100       | 9 × 20      |

    @validation @security
    Scenario: Markup in customer details is treated as text
      Given I am logged in as "standard_user" with "Sauce Labs Bike Light" in the cart
      When I enter first name "<script>alert(1)</script>", last name "Lovelace" and postal code "1000"
      And I click "Continue"
      Then no script runs and no dialog appears
      And I reach "Checkout: Overview"

  # ================================================================ login
  Rule: Login gives precise feedback and accepts only exact credentials

    @login
    Scenario Outline: Credentials are matched exactly
      When I log in as "<user name>" with the password shown on the login page
      Then I see the error "Epic sadface: Username and password do not match any user in this service"

      Examples:
        | user name      |
        | Standard_User  |
        | standard_user␣ |
        | ␣standard_user |

    @login @accessibility
    Scenario: The whole purchase can be completed with the keyboard only
      Given I am logged in as "standard_user"
      When I use only Tab, Shift+Tab, Enter and Space
      Then I can add a product, open the cart, check out and finish the order
      And the focused element is always visible
