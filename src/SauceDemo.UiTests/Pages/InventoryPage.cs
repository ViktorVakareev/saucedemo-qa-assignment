using SauceDemo.UiTests.Models;

namespace SauceDemo.UiTests.Pages;

public enum SortOption { NameAToZ, NameZToA, PriceLowToHigh, PriceHighToLow }

public sealed class InventoryPage(IPage page)
{
    private ILocator Items => page.Locator("[data-test='inventory-item']");
    private ILocator SortDropdown => page.Locator("[data-test='product-sort-container']");

    public ILocator Title => page.Locator("[data-test='title']");
    public HeaderComponent Header => new(page);

    public async Task<IReadOnlyList<Product>> GetProductsAsync()
    {
        await Items.First.WaitForAsync();
        var products = new List<Product>();
        foreach (var item in await Items.AllAsync())
        {
            var name = await item.Locator("[data-test='inventory-item-name']").InnerTextAsync();
            var price = await item.Locator("[data-test='inventory-item-price']").InnerTextAsync();
            products.Add(new Product(name.Trim(), Money.Parse(price)));
        }
        return products;
    }

    public async Task<IReadOnlyList<string>> GetProductNamesAsync() =>
        (await GetProductsAsync()).Select(p => p.Name).ToList();

    public Task AddToCartAsync(string productName) =>
        Item(productName).GetByRole(AriaRole.Button, new() { Name = "Add to cart" }).ClickAsync();

    public Task SortByAsync(SortOption option) =>
        SortDropdown.SelectOptionAsync(option switch
        {
            SortOption.NameAToZ => "az",
            SortOption.NameZToA => "za",
            SortOption.PriceLowToHigh => "lohi",
            SortOption.PriceHighToLow => "hilo",
            _ => throw new ArgumentOutOfRangeException(nameof(option)),
        });

    private ILocator Item(string productName) =>
        Items.Filter(new() { Has = page.GetByText(productName, new() { Exact = true }) });
}
