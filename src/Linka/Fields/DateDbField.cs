using System.Globalization;

namespace Webamoki.Linka.Fields;

public class DateTimeValidator : Validator
{
    private readonly DateTime _minDate;
    private readonly DateTime _maxDate;
    private readonly bool _isDateOnly;

    private DateTimeValidator(DateTime minDate, DateTime maxDate, bool isDateOnly)
    {
        if (minDate > maxDate)
            throw new ArgumentException("Min date cannot be after max date");

        _minDate = minDate;
        _maxDate = maxDate;
        _isDateOnly = isDateOnly;
    }

    public static DateTimeValidator Create(
        DateTime? minDate = null,
        DateTime? maxDate = null,
        bool isDateOnly = false)
    {
        var now = DateTime.Now;
        var min = minDate ?? now.AddYears(-80);
        var max = maxDate ?? now.AddYears(80);

        var hash = $"{min:yyyyMMddHHmmss}:{max:yyyyMMddHHmmss}:{isDateOnly}";
        if (Load<DateTimeValidator>(hash, out var validator))
            return validator!;

        validator = new DateTimeValidator(min, max, isDateOnly);
        Register(hash, validator);
        return validator;
    }

    public override bool IsValid(object? value, out string? message)
    {
        if (value is not string str)
        {
            message = "Value is not a string";
            return false;
        }

        var primaryFormat = _isDateOnly ? "yyyy-MM-dd" : "yyyy-MM-dd HH:mm:ss";
        const string fallbackFormat = "yyyy-MM-dd"; // allow date-only even if expecting datetime

        if (DateTime.TryParseExact(str, primaryFormat, CultureInfo.InvariantCulture, DateTimeStyles.None,
                out var parsed)
            || (!_isDateOnly && DateTime.TryParseExact(str, fallbackFormat, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out parsed)))
        {
        }

        message = null;
        return parsed >= _minDate && parsed <= _maxDate;
    }

    public static bool LessThanOrEqual(DateTime left, string right)
    {
        var rightDt = DateTime.Parse(right);
        return left <= rightDt;
    }

    public static bool GreaterThanOrEqual(DateTime left, string right)
    {
        var rightDt = DateTime.Parse(right);
        return left >= rightDt;
    }

    public static bool LessThan(DateTime left, string right)
    {
        var rightDt = DateTime.Parse(right);
        return left < rightDt;
    }

    public static bool GreaterThan(DateTime left, string right)
    {
        var rightDt = DateTime.Parse(right);
        return left > rightDt;
    }
}

public class DateTimeDbField : RefDbField<DateTime>
{
    protected DateTimeDbField(DateTime? minDate, DateTime? maxDate, bool isDateOnly)
        : base(
            DateTimeValidator.Create(minDate, maxDate, isDateOnly), isDateOnly ? "DATE" : "TIMESTAMP(0)") { }

    public DateTimeDbField(DateTime? minDate = null, DateTime? maxDate = null) : this(minDate, maxDate, false) { }
    public override string StringValue() => Value().ToString("yyyy-MM-dd HH:mm:ss");

    public override object ObjectValue() => Value();
    
    public override void LoadValue(object? value)
    {
        switch (value)
        {
            case DateTime dateTime:
                Value(dateTime);
                return;
            case string str:
                Validator.IsValid(str, out _);
                Value(DateTime.Parse(str));
                return;
            default:
                throw new Exception("Value is not a valid datetime string");
        }
    }

    public void SetNow()
    {
        Value(DateTime.Now);
    }
    public static bool operator <=(DateTimeDbField left, string right) =>
        DateTimeValidator.LessThanOrEqual(left.Value(), right);
    
    public static bool operator >=(DateTimeDbField left, string right) =>
        DateTimeValidator.GreaterThanOrEqual(left.Value(), right);
    
    public static bool operator <(DateTimeDbField left, string right) =>
        DateTimeValidator.LessThan(left.Value(), right);
    
    public static bool operator >(DateTimeDbField left, string right) =>
        DateTimeValidator.GreaterThan(left.Value(), right);
    
}

public class DateDbField(DateTime? minDate = null, DateTime? maxDate = null) : DateTimeDbField(minDate, maxDate, true)
{
    public override void LoadValue(object? value)
    {
        if (value is DateTime dateTime)
        {
            Value(dateTime);
            return;
        }

        throw new Exception("Value is not a valid datetime string");
    }
    public override string StringValue() => Value().ToString("yyyy-MM-dd");
}