using Webamoki.Linka.Expressions;
using Webamoki.Linka.ModelSystem;

namespace Webamoki.Linka.Fields.NumericFields;

public class IntValidator : Validator
{
    private readonly int _max;
    private readonly int _min;

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

    internal override object ObjectValue() => Value() ?? throw new InvalidOperationException("Value is null");

    private static string GetSqlType(int min, int max)
    {
        var sqlType = (min, max) switch
        {
            //postgressql
            ( >= -32768, <= 32767) => "SMALLINT",
            _ => "INT"
        };
        return sqlType;
    }

    public static bool operator <=(IntDbField left, int right) => (left.Value() ?? throw new InvalidOperationException()) <= right;
    public static bool operator >=(IntDbField left, int right) => (left.Value() ?? throw new InvalidOperationException()) >= right;
    public static bool operator >(IntDbField left, int right) => (left.Value() ?? throw new InvalidOperationException()) > right;
    public static bool operator <(IntDbField left, int right) => (left.Value() ?? throw new InvalidOperationException()) < right;

    internal override ConditionEx<TU> ParseEx<TU>(string op, object value) =>
        new IntEx<TU>(Name, op, (int)value);
    internal override string GetUpdateSetQuery<TSchema>(object value, out object? queryValue)
    {
        queryValue = (int)value;
        return $"{value}";
    }
}

internal record IntEx<T>(string Name, string Op, int Value) : ConditionEx<T>(Name) where T : Model
{
    public override string ToQuery(out List<object> values)
    {
        values = [];
        return $"{GetName()} {Op} {Value}";
    }

    public override bool Verify(T model)
    {
        var value = (int)GetValue(model);
        return Op switch
        {
            "=" => value == Value,
            "!=" => value != Value,
            ">" => value > Value,
            "<" => value < Value,
            ">=" => value >= Value,
            "<=" => value <= Value,
            _ => throw new NotSupportedException($"Operator {Op} is not supported for int fields.")
        };
    }
}