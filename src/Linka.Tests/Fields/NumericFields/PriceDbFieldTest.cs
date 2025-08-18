using NUnit.Framework;
using Webamoki.Linka.Fields.NumericFields;
using Webamoki.TestUtils;

namespace Linka.Tests.Fields.NumericFields;

public class PriceDbFieldTest
{
    [Test]
    public void Display_FormatsUsdCorrectly()
    {
        var priceField = new PriceDbField<Usd>();
        priceField.Value(123456);
        var price = priceField.Price;
        Ensure.Equal(123456, price.MinorUnits);
        Ensure.Equal("$1,234.56", price.Display());
        Ensure.Equal("1,234.56", price.Display(false));
    }

    [Test]
    public void Display_FormatsEurCorrectly()
    {
        var priceField = new PriceDbField<Eur>();
        priceField.Value(123456);
        var price = priceField.Price;
        Ensure.Equal(123456, price.MinorUnits);
        Ensure.Equal("1.234,56€", price.Display());
        Ensure.Equal("1.234,56", price.Display(false));
    }

    [Test]
    public void Display_FormatsGbpCorrectly()
    {
        var priceField = new PriceDbField<Gbp>();
        priceField.Value(123456);
        var price = priceField.Price;
        Ensure.Equal(123456, price.MinorUnits);
        Ensure.Equal("£1,234.56", price.Display());
        Ensure.Equal("1,234.56", price.Display(false));
    }

    [Test]
    public void Display_FormatsJpyCorrectly()
    {
        var priceField = new PriceDbField<Jpy>();
        priceField.Value(123456);
        var price = priceField.Price;
        Ensure.Equal(123456, price.MinorUnits);
        Ensure.Equal("¥123,456", price.Display());
        Ensure.Equal("123,456", price.Display(false));
    }

    [Test]
    public void Display_FormatsNegativeEurCorrectly()
    {
        var priceField = new PriceDbField<Eur>();
        priceField.Value(-123456);
        var price = priceField.Price;
        Ensure.Equal(-123456, price.MinorUnits);
        Ensure.Equal("-1.234,56€", price.Display());
        Ensure.Equal("-1.234,56", price.Display(false));
    }
}