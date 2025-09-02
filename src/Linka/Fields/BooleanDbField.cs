using Webamoki.Linka.Expressions;
using Webamoki.Linka.ModelSystem;

namespace Webamoki.Linka.Fields;

public sealed class BooleanValueValidator : Validator
{
    private BooleanValueValidator() { }

    public static BooleanValueValidator Create()
    {
        if (Load<BooleanValueValidator>("bool", out var validator))
            return validator!;

        validator = new BooleanValueValidator();
        Register("bool", validator);
        return validator;
    }

    public override bool IsValid(object? value, out string? message)
    {
        if (value is not bool)
        {
            message = "Value is not a boolean";
            return false;
        }

        message = null;
        return true;
    }
}

public class BooleanDbField() : StructDbField<bool>(BooleanValueValidator.Create(), "BOOLEAN")
{
    public override string StringValue()
    {
        var value = Value() ?? throw new InvalidOperationException("Value is null");
        return value ? "true" : "false";
    }

    public override object ObjectValue()
    {
        var value = Value() ?? throw new InvalidOperationException("Value is null");
        return value;
    }

    internal override ConditionEx<T> ParseEx<T>(string op, object value)
    {
        return op switch
        {
            "=" => new BoolEx<T>(Name, true, (bool)value),
            "!=" => new BoolEx<T>(Name, false, (bool)value),
            _ => throw new NotSupportedException($"Operator {op} is not supported for boolean fields.")
        };
    }

    internal override string GetUpdateSetQuery<TSchema>(object value, out object? queryValue)
    {
        queryValue = null;
        return $"{value}";
    }

}
internal record BoolEx<T>(string Name, bool IsEqual, bool Value) : ConditionEx<T>(Name) where T : Model
{
    public override string ToQuery(out List<object> values)
    {
        var op = IsEqual ? "=" : "!=";
        values = [];
        var value = Value ? "true" : "false";
        return $"{GetName()} {op} {value}";
    }

    public override bool Verify(T model)
    {
        var value = (bool)GetValue(model);
        return IsEqual ? value == Value : value != Value;
    }
}