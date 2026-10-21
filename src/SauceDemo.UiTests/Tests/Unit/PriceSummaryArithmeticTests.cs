using System.Globalization;

namespace SauceDemo.UiTests.Tests.Unit;

/// <summary>
/// Backs the analysis in BugReport.md (BUG-001) with something anyone can run: replays the site's
/// price-summary arithmetic for every cart (every combination and add-order of the 6 products) in
/// IEEE double, which is what JavaScript numbers are, and compares it with exact decimal arithmetic.
/// Catalogue prices as listed on the inventory page on 2026-10-21.
/// </summary>
[TestFixture]
[Category("Unit")]
public sealed class PriceSummaryArithmeticTests
{
    private static readonly double[] CataloguePrices = [29.99, 9.99, 15.99, 49.99, 7.99, 15.99];
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    [Test]
    public void Tax_and_total_are_correct_for_every_cart()
    {
        var carts = AllCarts().ToList();
        var wrong = carts.Where(cart =>
        {
            var (_, tax, total) = SiteSummary(cart);
            var exact = cart.Sum(p => (decimal)p);
            var exactTax = Math.Round(exact * 0.08m, 2, MidpointRounding.AwayFromZero);
            return tax != exactTax || total != exact + exactTax;
        }).ToList();

        Assert.That(carts, Has.Count.EqualTo(1956), "6 products: every non-empty subset in every order");
        Assert.That(wrong, Is.Empty, "Tax = round(item total x 8%, 2) and Total = item total + Tax");
    }

    [Test]
    public void Item_total_display_shows_floating_point_noise_for_some_carts()
    {
        Assert.That(SiteSummary([9.99, 49.99]).ItemTotalShown, Is.EqualTo("59.980000000000004"));   // BUG-001
        Assert.That(SiteSummary([7.99, 49.99]).ItemTotalShown, Is.EqualTo("57.980000000000004"));

        var noisy = AllCarts().Count(cart => SiteSummary(cart).ItemTotalShown.Split('.').ElementAtOrDefault(1)?.Length > 2);
        TestContext.Progress.WriteLine($"carts with a noisy Item total: {noisy} of 1956");
        Assert.That(noisy, Is.GreaterThan(0));
    }

    [Test]
    public void The_same_products_can_show_the_defect_or_not_depending_on_add_order()
    {
        Assert.That(SiteSummary([49.99, 9.99, 29.99, 15.99]).ItemTotalShown, Is.EqualTo("105.96"));
        Assert.That(SiteSummary([29.99, 9.99, 15.99, 49.99]).ItemTotalShown, Is.EqualTo("105.96000000000001"));
    }

    /// <summary>The site's calculation: running double sum shown as is; tax and total via toFixed(2).</summary>
    private static (string ItemTotalShown, decimal Tax, decimal Total) SiteSummary(IReadOnlyList<double> cartInAddOrder)
    {
        var sum = 0.0;
        foreach (var price in cartInAddOrder) sum += price;
        var tax = (sum * 0.08).ToString("F2", Inv);
        var total = (sum + double.Parse(tax, Inv)).ToString("F2", Inv);
        return (sum.ToString(Inv), decimal.Parse(tax, Inv), decimal.Parse(total, Inv));
    }

    private static IEnumerable<double[]> AllCarts()
    {
        for (var mask = 1; mask < 1 << CataloguePrices.Length; mask++)
        {
            var chosen = Enumerable.Range(0, CataloguePrices.Length).Where(i => (mask >> i & 1) == 1).Select(i => CataloguePrices[i]).ToArray();
            foreach (var order in Permutations(chosen)) yield return order;
        }
    }

    private static IEnumerable<double[]> Permutations(double[] items)
    {
        if (items.Length <= 1) { yield return items; yield break; }
        for (var i = 0; i < items.Length; i++)
        {
            var rest = items.Where((_, j) => j != i).ToArray();
            foreach (var tail in Permutations(rest)) yield return [items[i], .. tail];
        }
    }
}
