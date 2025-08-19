using NUnit.Framework;
using Webamoki.Linka.Fields;
using Webamoki.Utils.Testing;

namespace Tests.Fields;

public class DateTimeDbFieldTest
{
    [Test]
    public void Validator_CachesCorrectly()
    {
        var min = DateTime.Today;
        var max = DateTime.Today.AddYears(1);

        var v1 = DateTimeValidator.Create(min, max, true);
        var v2 = DateTimeValidator.Create(min, max, true);
        Ensure.Equal(v1, v2);
    }

    [Test]
    public void Validator_ThrowsIfMinAfterMax()
    {
        var min = DateTime.Today.AddDays(1);
        var max = DateTime.Today;

        Assert.Throws<ArgumentException>(() =>
            _ = DateTimeValidator.Create(min, max, true));
    }

    [TestCase("2022-04-25", true)]
    [TestCase("2025-12-31", true)]
    [TestCase("2031-01-01", false)] // too late
    [TestCase("2019-12-31", false)] // too early
    [TestCase("22-04-25", false)]   // wrong format
    [TestCase("2022/04/25", false)] // wrong delimiter
    public void Validator_ValidatesCorrectDateOnlyFormat(string input, bool expected)
    {
        var min = new DateTime(2020, 1, 1);
        var max = new DateTime(2030, 1, 1);
        var validator = DateTimeValidator.Create(min, max, true);

        Ensure.Equal(expected, validator.IsValid(input, out _));
    }

    [TestCase("2022-04-25 10:30:00", true)]
    [TestCase("2020-01-01 00:00:00", true)]
    [TestCase("2030-01-01 23:59:59", false)]
    [TestCase("2022-04-25", true)]             // missing time but okay
    [TestCase("2022/04/25 10:30:00", false)]    // wrong delimiter
    [TestCase("2022-04-25T10:30:00", false)]    // incorrect format
    [TestCase("2019-12-31 23:59:59", false)]    // too early
    [TestCase("2035-01-01 00:00:00", false)]    // too late
    public void Validator_ValidatesCorrectDateTimeFormat(string input, bool expected)
    {
        var min = new DateTime(2020, 1, 1);
        var max = new DateTime(2030, 1, 1);
        var validator = DateTimeValidator.Create(min, max);

        Ensure.Equal(expected, validator.IsValid(input, out _));
    }

    [TestCase("2022-04-25 10:30:00", false)] // too late
    [TestCase("2019-12-31 23:59:59", false)] // too early
    [TestCase("2020-06-01 12:00:00", true)]  // in range
    public void Validator_RejectsOutOfRangeDates(string input, bool expected)
    {
        var min = new DateTime(2020, 1, 1);
        var max = new DateTime(2021, 1, 1);
        var validator = DateTimeValidator.Create(min, max);

        Ensure.Equal(expected, validator.IsValid(input, out _));
    }

    [Test]
    public void DateTimeDbField_StringValue_ThrowsIfUnset()
    {
        var field = new DateTimeDbField();
        Ensure.True(field.IsEmpty());
        Ensure.Throws<InvalidOperationException>(() => _ = field.StringValue());
    }

    [Test]
    public void DateTimeDbField_StringValue_ReturnsCorrectValue()
    {
        var field = new DateTimeDbField();
        field.Value("2025-04-22 00:00:00");
        Ensure.Equal("2025-04-22 00:00:00", field.StringValue());
    }

    [Test]
    public void DateDbField_AcceptsDateOnlyFormat()
    {
        var field = new DateDbField(new DateTime(2000, 1, 1), new DateTime(2100, 1, 1));
        field.Value("2025-04-22");
        Ensure.Equal("2025-04-22", field.StringValue());
    }

    [Test]
    public void ChangesVerifyCorrectly()
    {
        var field = new DateDbField();
        Ensure.True(field.IsEmpty());
        Ensure.Equal("DATE", field.SQLType);
        field.Value("2025-04-22");
        Ensure.False(field.IsEmpty());
        Ensure.True(field.IsSet);
        Ensure.False(field.IsChanged());
        field.Value(null);
        Ensure.True(field.IsEmpty());
        Ensure.True(field.IsSet);
        Ensure.True(field.IsChanged());
        field.ResetChange();
        Ensure.False(field.IsChanged());
        Ensure.Equal(null, field.Value());
        field.Value("2021-04-22");
        Ensure.True(field.IsChanged());
    }
}
