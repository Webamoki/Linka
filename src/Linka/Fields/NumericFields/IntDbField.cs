using Webamoki.Linka.Expressions.Ex;

namespace Webamoki.Linka.Fields.NumericFields;

public class IntValidator : Validator
{
    private readonly int _min;
    private readonly int _max;

    private IntValidator(int min, int max)
    {
        if (max < min)
            throw new ArgumentOutOfRangeException(nameof(max), "Max must be greater than or equal to Min");
        _min = min;
        _max = max;
    }

    public static IntValidator Create(int min = -99, int max = 99)
    {
        var hash = $"{min}:{max}";
        if (Load<IntValidator>(hash, out var validator))
            return validator!;
        
        validator = new IntValidator(min, max);
        Register(hash, validator);
        return validator;
    }

    public override bool IsValid(object? value, out string? message)
    {
        if (value is not int i)
        {
            message = "Value is not an integer";
            return false;
        }

        if (i < _min || i > _max)
        {
            message = $"Value {i} is out of range [{_min}, {_max}]";
            return false;
        }

        message = null;
        return true;
    }
}
public class IntDbField(int min, int max) : StructDbField<int>(IntValidator.Create(min, max),
    $"{GetSqlType(min, max)}")
{
    public override string StringValue() => Value().ToString() ?? throw new InvalidOperationException();

    public override object ObjectValue()
    {
        var value = Value() ?? throw new InvalidOperationException("Value is null");
        return value;
    }
    
    private static string GetSqlType(int min, int max)
    {
        var sqlType = (min, max) switch
        {
            //postgressql
            (>= -32768, <= 32767) => "SMALLINT",
            _ => "INT"
        };
        return sqlType;
    }

    public static bool operator <=(IntDbField left, int right) => (left.Value() ?? throw new InvalidOperationException()) <= right;
    public static bool operator >=(IntDbField left, int right) => (left.Value() ?? throw new InvalidOperationException()) >= right;
    public static bool operator >(IntDbField left, int right) => (left.Value() ?? throw new InvalidOperationException()) > right;
    public static bool operator <(IntDbField left, int right) => (left.Value() ?? throw new InvalidOperationException()) < right;

    internal override IConditionEx<TU> ParseEx<TU>(string op, object value) =>
        new IntEx<TU>(Name, op, (int)value);
}
