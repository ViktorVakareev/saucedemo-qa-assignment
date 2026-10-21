# Manual test cases for the SauceDemo purchase flow.
# Written in Gherkin for test design only; they are not wired to a BDD runner.
#
# Test data: any user listed on the login page; the password shown there ("secret_sauce" at the time of writing).
# Expected results where the site gives no specification (marked "assumption") follow common e-commerce practice
# and would be confirmed with the product owner.

Feature: Purchase flow on SauceDemo
  As a shopper
  I want to log in, put products in my cart and check out
  So that I can buy products

  Background:
    Given I open the SauceDemo login page in a new browser session

  # ---------------------------------------------------------------- positive
  @positive @smoke
  Scenario: Standard user buys one product end to end
    Given I log in as "standard_user" with the password shown on the login page
    And I see the "Products" page with 6 products
    When I click "Add to cart" for "Sauce Labs Backpack"
    Then the button for "Sauce Labs Backpack" changes to "Remove"
    And the cart badge shows "1"
    When I open the cart
    Then the cart lists "Sauce Labs Backpack" with quantity 1 and price "$29.99"
    When I click "Checkout"
    And I enter first name "Ada", last name "Lovelace" and postal code "1000"
    And I click "Continue"
    Then the overview lists "Sauce Labs Backpack"
    And "Item total" is "$29.99", "Tax" is "$2.40" and "Total" is "$32.39"
    When I click "Finish"
    Then I see "Thank you for your order!"
    And the cart badge is not shown
    When I open the menu and click "Logout"
    Then I am on the login page
    And opening "/inventory.html" shows "You can only access '/inventory.html' when you are logged in."

  # ---------------------------------------------------------------- negative 1
  @negative @login
  Scenario: Locked-out user cannot log in
    When I log in as "locked_out_user" with the password shown on the login page
    Then I stay on the login page
    And I see the error "Epic sadface: Sorry, this user has been locked out."
    And the products page is not reachable without logging in

  # ---------------------------------------------------------------- negative 2
  @negative @checkout
  Scenario Outline: Checkout stops when a required customer field is missing
    Given I am logged in as "standard_user" with "Sauce Labs Bike Light" in the cart
    And I am on the "Checkout: Your Information" page
    When I enter first name "<first name>", last name "<last name>" and postal code "<postal code>"
    And I click "Continue"
    Then I stay on the "Checkout: Your Information" page
    And I see the error "<error>"
    And the values I entered are kept in the form

    Examples:
      | first name | last name | postal code | error                          |
      |            | Lovelace  | 1000        | Error: First Name is required  |
      | Ada        |           | 1000        | Error: Last Name is required   |
      | Ada        | Lovelace  |             | Error: Postal Code is required |

  # ---------------------------------------------------------------- edge case
  @edge @cart @checkout
  Scenario: An order cannot be placed with an empty cart
    # Edge of the cart: zero items. Assumption (no spec): an empty order shouldn't be possible.
    Given I am logged in as "standard_user"
    And my cart is empty
    When I open the cart
    Then the cart shows no items
    And "Checkout" is disabled, or checking out shows a message that the cart is empty
    And no "Checkout: Overview" with "Item total: $0" is shown
    And no order confirmation is shown
