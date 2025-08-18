using NUnit.Framework;
using Webamoki.Linka.Fields.NumericFields;
using Webamoki.TestUtils;

namespace LinkaTests.Fields.NumericFields;

public class IntDbFieldTest
{
    // --- IntValidator: Valid Values ---
    [TestCase(0, 99, 0)]
    [TestCase(0, 99, 99)]
    [TestCase(-10, 10, -10)]
    [TestCase(-10, 10, 0)]
    [TestCase(-10, 10, 10)]
    public void IntValidator_ValidValues_Pass(int min, int max, int value)
    {
        var validator = IntValidator.Create(min, max);
        Ensure.Equal(validator.IsValid(value, out _), true);
    }

    // --- IntValidator: Invalid Values ---
    [TestCase(0, 99, -1)]
    [TestCase(0, 99, 100)]
    [TestCase(-10, 10, -11)]
    [TestCase(-10, 10, 11)]
    [TestCase(0, 99, null)]
    [TestCase(0, 99, "abc")]
    public void IntValidator_InvalidValues_Fail(int min, int max, object? value)
    {
        var validator = IntValidator.Create(min, max);
        Ensure.Equal(validator.IsValid(value, out _), false);
    }

    // --- IntDbField: SQL Type and Size ---
    [TestCase(-128, 127, "SMALLINT")]
    [TestCase(-32768, 32767, "SMALLINT")]
    [TestCase(-8388608, 8388607, "INT")]
    [TestCase(-1000, 100000, "INT")]
    [TestCase(-1000, int.MaxValue, "INT")]
    public void IntDbField_SQLTypeAndSize_AreCorrect(int min, int max, string expectedSqlType)
    {
        var field = new IntDbField(min, max);
        Ensure.Equal($"{expectedSqlType}", field.SQLType);
    }

    [Test]
    public void IntDbField_StringValue_ReturnsExpected()
    {
        var field = new IntDbField(0, 100);
        field.Value(42);
        Ensure.Equal(field.StringValue(), "42");
    }
}