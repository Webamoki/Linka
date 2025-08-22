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

    [Test]
    public void SetNow_SetsCurrentDateTime()
    {
        var field = new DateTimeDbField();
        var beforeCall = DateTime.Now;

        field.SetNow();

        var afterCall = DateTime.Now;
        var fieldValue = field.Value();

        Ensure.NotNull(fieldValue);
        Ensure.True(field.IsSet);
        Ensure.False(field.IsEmpty());

        // Parse the field value to verify it's a valid datetime
        var parsedDateTime = DateTime.ParseExact(fieldValue!, "yyyy-MM-dd HH:mm:ss", null);

        // Verify the datetime is within a reasonable range (within 1 second of when we called SetNow)
        Ensure.True(parsedDateTime >= beforeCall.AddSeconds(-1));
        Ensure.True(parsedDateTime <= afterCall.AddSeconds(1));
    }

    [Test]
    public void SetNow_SetsFieldAsChanged_WhenPreviouslySet()
    {
        var field = new DateTimeDbField();
        field.Value("2020-01-01 00:00:00");
        field.ResetChange(); // Reset change tracking

        Ensure.False(field.IsChanged());

        field.SetNow();

        Ensure.True(field.IsChanged());
    }

    [Test]
    public void SetNow_SetsFieldAsNotChanged_WhenFirstTimeSet()
    {
        var field = new DateTimeDbField();
        Ensure.True(field.IsEmpty());

        field.SetNow();

        Ensure.False(field.IsChanged()); // First time setting should not be marked as changed
        Ensure.True(field.IsSet);
    }

    [Test]
    public void SetNow_OverwritesPreviousValue()
    {
        var field = new DateTimeDbField();
        field.Value("2020-01-01 12:00:00");
        var originalValue = field.Value();

        // Wait a small amount to ensure different timestamp
        Thread.Sleep(10);

        field.SetNow();
        var newValue = field.Value();

        Ensure.NotEqual(originalValue, newValue);
        Ensure.True(field.IsSet);
    }

    [Test]
    public void SetNow_ProducesValidStringValue()
    {
        var field = new DateTimeDbField();
        field.SetNow();

        var stringValue = field.StringValue();

        // Verify the string value matches the expected format
        Ensure.True(DateTime.TryParseExact(stringValue, "yyyy-MM-dd HH:mm:ss", null,
            System.Globalization.DateTimeStyles.None, out _));
    }

    [Test]
    public void SetNow_WorksWithinValidatorConstraints()
    {
        var minDate = DateTime.Now.AddDays(-1);
        var maxDate = DateTime.Now.AddDays(1);
        var field = new DateTimeDbField(minDate, maxDate);

        field.SetNow();

        Ensure.True(field.IsValid(out var message));
        Ensure.Null(message);
    }

    [Test]
    public void SetNow_FailsValidationIfOutsideConstraints()
    {
        // Create a field with constraints that exclude "now"
        var minDate = DateTime.Now.AddDays(-10);
        var maxDate = DateTime.Now.AddDays(-5);
        var field = new DateTimeDbField(minDate, maxDate);

        field.SetNow();

        // The field should be set but validation should fail
        Ensure.True(field.IsSet);
        Ensure.False(field.IsValid(out var message));
        Ensure.Null(message);
    }

    [Test]
    public void SetNow_MultipleCallsProduceDifferentValues()
    {
        var field = new DateTimeDbField();

        field.SetNow();
        var firstValue = field.Value();

        // Wait a small amount to ensure different timestamp
        Thread.Sleep(1000);

        field.SetNow();
        var secondValue = field.Value();

        // Values should be different 
        Ensure.NotEqual(firstValue, secondValue);
    }

    [Test]
    public void DateDbField_SetNow_SetsCurrentDate()
    {
        var field = new DateDbField();
        var beforeCall = DateTime.Now.Date;

        field.SetNow();

        var afterCall = DateTime.Now.Date;
        var fieldValue = field.Value();

        Ensure.NotNull(fieldValue);
        Ensure.True(field.IsSet);
        Ensure.False(field.IsEmpty());

        // Parse the field value to verify it's a valid date
        var parsedDate = DateTime.ParseExact(fieldValue!, "yyyy-MM-dd", null);

        // Verify the date is today (should be the same before and after unless we cross midnight)
        Ensure.True(parsedDate.Date == beforeCall || parsedDate.Date == afterCall);
    }

    [Test]
    public void DateDbField_SetNow_UsesDateOnlyFormat()
    {
        var field = new DateDbField();
        field.SetNow();

        var fieldValue = field.Value();
        var stringValue = field.StringValue();

        // Verify the format is date-only (yyyy-MM-dd)
        Ensure.Equal(fieldValue, stringValue);
        Ensure.True(DateTime.TryParseExact(stringValue, "yyyy-MM-dd", null,
            System.Globalization.DateTimeStyles.None, out _));

        // Ensure it doesn't contain time information
        Ensure.False(stringValue.Contains(" "));
        Ensure.False(stringValue.Contains(":"));
    }

    [Test]
    public void DateDbField_SetNow_SetsFieldAsChanged_WhenPreviouslySet()
    {
        var field = new DateDbField();
        field.Value("2020-01-01");
        field.ResetChange(); // Reset change tracking

        Ensure.False(field.IsChanged());

        field.SetNow();

        Ensure.True(field.IsChanged());
    }

    [Test]
    public void DateDbField_SetNow_WorksWithinValidatorConstraints()
    {
        var minDate = DateTime.Now.AddDays(-1);
        var maxDate = DateTime.Now.AddDays(1);
        var field = new DateDbField(minDate, maxDate);

        field.SetNow();

        Ensure.True(field.IsValid(out var message));
        Ensure.Null(message);
    }

    [Test]
    public void DateDbField_SetNow_OverwritesPreviousValue()
    {
        var field = new DateDbField();
        field.Value("2020-01-01");
        var originalValue = field.Value();

        field.SetNow();
        var newValue = field.Value();

        Ensure.NotEqual(originalValue, newValue);
        Ensure.True(field.IsSet);

        // New value should be today's date
        var today = DateTime.Now.ToString("yyyy-MM-dd");
        Ensure.Equal(today, newValue);
    }

    [Test]
    public void DateTimeDbField_ObjectValue_ThrowsIfUnset()
    {
        var field = new DateTimeDbField();
        Ensure.True(field.IsEmpty());
        Ensure.Throws<InvalidOperationException>(() => _ = field.ObjectValue());
    }

    [TestCase("2024-01-15 10:30:00")]
    [TestCase("2023-12-25 00:00:00")]
    public void DateTimeDbField_ObjectValue_ReturnsSetValue(string value)
    {
        var field = new DateTimeDbField();
        field.Value(value);
        Ensure.Equal(value, field.ObjectValue());
    }

    [Test]
    public void DateDbField_ObjectValue_ThrowsIfUnset()
    {
        var field = new DateDbField();
        Ensure.True(field.IsEmpty());
        Ensure.Throws<InvalidOperationException>(() => _ = field.ObjectValue());
    }

    [TestCase("2024-01-15")]
    [TestCase("2023-12-25")]
    public void DateDbField_ObjectValue_ReturnsSetValue(string value)
    {
        var field = new DateDbField();
        field.Value(value);
        Ensure.Equal(value, field.ObjectValue());
    }
}
