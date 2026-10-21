using System.Text.Json;

namespace SauceDemo.UiTests.Config;

public sealed record CheckoutCustomer(string FirstName, string LastName, string PostalCode)
{
    public static CheckoutCustomer Load(string fileName = "checkout-customer.json")
    {
        var path = Path.Combine(AppContext.BaseDirectory, "TestData", fileName);
        return JsonSerializer.Deserialize<CheckoutCustomer>(File.ReadAllText(path))
               ?? throw new InvalidOperationException($"Test data file '{path}' is empty.");
    }
}
