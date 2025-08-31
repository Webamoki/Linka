using System.Globalization;
using Webamoki.Linka.Expressions;
using Webamoki.Linka.ModelSystem;

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

    public static bool LessThanOrEqual(string left, string right)
    {
        var leftDt = DateTime.Parse(left);
        var rightDt = DateTime.Parse(right);
        return leftDt <= rightDt;
    }

    public static bool GreaterThanOrEqual(string left, string right)
    {
        var leftDt = DateTime.Parse(left);
        var rightDt = DateTime.Parse(right);
        return leftDt >= rightDt;
    }

    public static bool LessThan(string left, string right)
    {
        var leftDt = DateTime.Parse(left);
        var rightDt = DateTime.Parse(right);
        return leftDt < rightDt;
    }

    public static bool GreaterThan(string left, string right)
    {
        var leftDt = DateTime.Parse(left);
        var rightDt = DateTime.Parse(right);
        return leftDt > rightDt;
    }
}

public class DateTimeDbField : RefDbField<string>
{
    protected DateTimeDbField(DateTime? minDate, DateTime? maxDate, bool isDateOnly)
        : base(
            DateTimeValidator.Create(minDate, maxDate, isDateOnly), isDateOnly ? "DATE" : "TIMESTAMP(0)") { }

    public DateTimeDbField(DateTime? minDate = null, DateTime? maxDate = null) : this(minDate, maxDate, false) { }
    public override string StringValue() => Value() ?? throw new InvalidOperationException("Value is null");

    public override object ObjectValue()
    {
        var value = Value() ?? throw new InvalidOperationException("Value is null");
        return DateTime.Parse(value);
    }
    
    public override void LoadValue(object? value)
    {
        switch (value)
        {
            case DateTime dateTime:
                Value(dateTime.ToString("yyyy-MM-dd HH:mm:ss"));
                return;
            case string str:
                Validator.IsValid(str, out _);
                Value(str);
                return;
            default:
                throw new Exception("Value is not a valid datetime string");
        }
    }

    public void SetNow()
    {
        Value(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
    }
    public static bool operator <=(DateTimeDbField left, string right) =>
        DateTimeValidator.LessThanOrEqual(left.Value() ?? throw new InvalidOperationException("Value is null"), right);
    
    public static bool operator >=(DateTimeDbField left, string right) =>
        DateTimeValidator.GreaterThanOrEqual(left.Value() ?? throw new InvalidOperationException("Value is null"), right);
    
    public static bool operator <(DateTimeDbField left, string right) =>
        DateTimeValidator.LessThan(left.Value() ?? throw new InvalidOperationException("Value is null"), right);
    
    public static bool operator >(DateTimeDbField left, string right) =>
        DateTimeValidator.GreaterThan(left.Value() ?? throw new InvalidOperationException("Value is null"), right);

    internal override ConditionEx<TU> ParseEx<TU>(string op, object value)
    {
        return value switch
        {
            DateTime dateTime => new DateTimeEx<TU>(Name, op, dateTime),
            string str => new DateTimeEx<TU>(Name, op, DateTime.Parse(str)),
            _ => throw new NotSupportedException($"Operator {op} is not supported for datetime fields.")
        };
    }
       
}

public class DateDbField(DateTime? minDate = null, DateTime? maxDate = null) : DateTimeDbField(minDate, maxDate, true)
{
    public override void LoadValue(object? value)
    {
        if (value is DateTime dateTime)
        {
            Value(dateTime.ToString("yyyy-MM-dd"));
            return;
        }

        throw new Exception("Value is not a valid datetime string");
    }

    public new void SetNow()
    {
        Value(DateTime.Now.ToString("yyyy-MM-dd"));
    }
    
    public override object ObjectValue()
    {
        var value = Value() ?? throw new InvalidOperationException("Value is null");
        return DateTime.Parse(value).Date;
    }
}

internal record DateTimeEx<T>(string Name, string Op, DateTime Value) : ConditionEx<T>(Name) where T : Model
{
    public override string ToQuery(out List<object> values)
    {
        values = [];
        return $"{GetName()} {Op} '{Value}'";
    }
    
    public override bool Verify(T model)
    {
        var value = (DateTime)GetValue(model);
        return Op switch
        {
            "=" => value == Value,
            "!=" => value != Value,
            ">" => value > Value,
            "<" => value < Value,
            ">=" => value >= Value,
            "<=" => value <= Value,
            _ => throw new NotSupportedException($"Operator {Op} is not supported for datetime fields.")
        };
    }
}