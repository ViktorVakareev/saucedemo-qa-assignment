namespace SauceDemo.UiTests.Pages;

public sealed class CartPage(IPage page)
{
    public ILocator Items => page.Locator("[data-test='inventory-item']");
    private ILocator ItemNames => page.Locator("[data-test='inventory-item-name']");
    private ILocator CheckoutButton => page.Locator("[data-test='checkout']");
    private ILocator ContinueShoppingButton => page.Locator("[data-test='continue-shopping']");

    public HeaderComponent Header => new(page);

    public async Task<IReadOnlyList<string>> GetItemNamesAsync() =>
        (await ItemNames.AllInnerTextsAsync()).Select(n => n.Trim()).ToList();

    public Task RemoveAsync(string productName) =>
        Items.Filter(new() { Has = page.GetByText(productName, new() { Exact = true }) })
             .GetByRole(AriaRole.Button, new() { Name = "Remove" })
             .ClickAsync();

    public async Task<InventoryPage> ContinueShoppingAsync()
    {
        await ContinueShoppingButton.ClickAsync();
        return new InventoryPage(page);
    }

    public async Task<CheckoutInformationPage> CheckoutAsync()
    {
        await CheckoutButton.ClickAsync();
        return new CheckoutInformationPage(page);
    }
}
