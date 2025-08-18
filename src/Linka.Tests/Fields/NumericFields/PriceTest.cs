using NUnit.Framework;
using Webamoki.Linka.Fields.NumericFields;
using Webamoki.TestUtils;

namespace Linka.Tests.Fields.NumericFields;

public class PriceTests
{
    [Test]
    public void Display_FormatsUsdCorrectly()
    {
        var price = new Price<Usd>(123456);
        Ensure.Equal(123456, price.MinorUnits);
        Ensure.Equal("$1,234.56", price.Display());
        Ensure.Equal("1,234.56", price.Display(false));
    }

    [Test]
    public void Display_FormatsEurCorrectly()
    {
        var price = new Price<Eur>(123456);
        Ensure.Equal(123456, price.MinorUnits);
        Ensure.Equal("1.234,56€", price.Display());
        Ensure.Equal("1.234,56", price.Display(false));
    }

    [Test]
    public void Display_FormatsGbpCorrectly()
    {
        var price = new Price<Gbp>(123456);
        Ensure.Equal(123456, price.MinorUnits);
        Ensure.Equal("£1,234.56", price.Display());
        Ensure.Equal("1,234.56", price.Display(false));
    }

    [Test]
    public void Display_FormatsJpyCorrectly()
    {
        var price = new Price<Jpy>(123456);
        Ensure.Equal(123456, price.MinorUnits);
        Ensure.Equal("¥123,456", price.Display());
        Ensure.Equal("123,456", price.Display(false));
    }

    [Test]
    public void Display_FormatsNegativeEurCorrectly()
    {
        var price = new Price<Eur>(-123456);
        Ensure.Equal(-123456, price.MinorUnits);
        Ensure.Equal("-1.234,56€", price.Display());
        Ensure.Equal("-1.234,56", price.Display(false));
    }


    [TestCase(1000, 2000, 3000)] // USD addition
    [TestCase(500, -200, 300)] // With negative
    [TestCase(0, 1500, 1500)] // Adding to zero
    public void Addition_Operator_WorksCorrectly(int initial, int toAdd, int expected)
    {
        var price1 = new Price<Usd>(initial);
        var price2 = new Price<Usd>(toAdd);
        var result = price1 + price2;
        Ensure.Equal(expected, result.MinorUnits);
    }

    [TestCase(1000, 500, 500)] // USD subtraction
    [TestCase(500, 1000, -500)] // Results in negative
    [TestCase(0, 500, -500)] // Subtracting from zero
    public void Subtraction_Operator_WorksCorrectly(int initial, int toSubtract, int expected)
    {
        var price1 = new Price<Usd>(initial);
        var price2 = new Price<Usd>(toSubtract);
        var result = price1 - price2;
        Ensure.Equal(expected, result.MinorUnits);
    }

    [TestCase(1000, 2.0, 2000)] // USD multiplication
    [TestCase(1000, 0.5, 500)] // Fractional multiplication
    [TestCase(1000, 0, 0)] // Multiply by zero
    public void Multiplication_Operator_WorksCorrectly(int initial, double multiplier, int expected)
    {
        var price = new Price<Usd>(initial);
        var result = price * multiplier;
        Ensure.Equal(expected, result.MinorUnits);
    }

    [TestCase(1000, 2, 500)] // USD division
    [TestCase(1000, 0.5, 2000)] // Division by fraction
    [TestCase(1000, 0, 0)] // Division by zero
    public void Division_Operator_WorksCorrectly(int initial, double divisor, int expected)
    {
        var price = new Price<Usd>(initial);
        var result = price / divisor;
        Ensure.Equal(expected, result.MinorUnits);
    }

    [TestCase(1000, 1000, true)] // Equal
    [TestCase(1000, 2000, false)] // Not equal
    [TestCase(0, 0, true)] // Zero equality
    public void Equality_Operators_WorkCorrectly(int value1, int value2, bool expected)
    {
        var price1 = new Price<Usd>(value1);
        var price2 = new Price<Usd>(value2);

        Ensure.Equal(expected, price1 == price2);
        Ensure.Equal(!expected, price1 != price2);
    }
}