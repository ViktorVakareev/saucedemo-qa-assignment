using SauceDemo.UiTests.Config;
using SauceDemo.UiTests.Models;

namespace SauceDemo.UiTests.Pages;

/// <summary>Checkout step one: "Your Information".</summary>
public sealed class CheckoutInformationPage(IPage page)
{
    public async Task<CheckoutOverviewPage> ContinueAsync(CheckoutCustomer customer)
    {
        await page.Locator("[data-test='firstName']").FillAsync(customer.FirstName);
        await page.Locator("[data-test='lastName']").FillAsync(customer.LastName);
        await page.Locator("[data-test='postalCode']").FillAsync(customer.PostalCode);
        await page.Locator("[data-test='continue']").ClickAsync();
        return new CheckoutOverviewPage(page);
    }
}

/// <summary>Checkout step two: "Overview" with item, tax and total amounts.</summary>
public sealed class CheckoutOverviewPage(IPage page)
{
    public ILocator Items => page.Locator("[data-test='inventory-item']");
    private ILocator ItemNames => page.Locator("[data-test='inventory-item-name']");
    private ILocator ItemPrices => page.Locator("[data-test='inventory-item-price']");

    public async Task<IReadOnlyList<string>> GetItemNamesAsync() =>
        (await ItemNames.AllInnerTextsAsync()).Select(n => n.Trim()).ToList();

    public async Task<IReadOnlyList<decimal>> GetItemPricesAsync() =>
        (await ItemPrices.AllInnerTextsAsync()).Select(Money.Parse).ToList();

    public ILocator ItemTotalLabel => page.Locator("[data-test='subtotal-label']");

    public async Task<decimal> GetItemTotalAsync() => Money.Parse(await ItemTotalLabel.InnerTextAsync());
    public async Task<decimal> GetTaxAsync() => Money.Parse(await page.Locator("[data-test='tax-label']").InnerTextAsync());
    public async Task<decimal> GetTotalAsync() => Money.Parse(await page.Locator("[data-test='total-label']").InnerTextAsync());

    public async Task<CheckoutCompletePage> FinishAsync()
    {
        await page.Locator("[data-test='finish']").ClickAsync();
        return new CheckoutCompletePage(page);
    }
}

/// <summary>Checkout complete: the order confirmation.</summary>
public sealed class CheckoutCompletePage(IPage page)
{
    public ILocator ConfirmationHeader => page.Locator("[data-test='complete-header']");
    public HeaderComponent Header => new(page);
}
