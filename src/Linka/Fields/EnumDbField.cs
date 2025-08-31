using Webamoki.Linka.Expressions;
using Webamoki.Linka.ModelSystem;
using Webamoki.Linka.SchemaSystem;

namespace Webamoki.Linka.Fields;

public class EnumValidator<TEnum> : Validator where TEnum : struct, Enum
{   
    private readonly HashSet<string> _enumNames = [];

    private EnumValidator()
    {
        foreach (var name in Enum.GetNames(typeof(TEnum)))
        {
            _enumNames.Add(name);
        }
    }

    public static EnumValidator<TEnum> Create()
    {
        var hash = typeof(TEnum).FullName!;
        if (Load<EnumValidator<TEnum>>(hash, out var validator))
            return validator!;
        validator = new EnumValidator<TEnum>();
        Register(hash, validator);
        return validator;
    }

    public override bool IsValid(object? value, out string? message)
    {
        if (value is string strValue)
        {
            if (_enumNames.Contains(strValue))
            {
                message = null;
                return true;
            }
            message = $"Value '{strValue}' is not a valid enum name of type {typeof(TEnum).Name}";
            return false;
        }
        if (value is not TEnum)
        {
            message = $"Value is not a valid enum of type {typeof(TEnum).Name}";
            return false;
        }

        message = null;
        return Enum.IsDefined(typeof(TEnum), value);
    }
}

internal interface IEnumDbField
{
    string GetSchemaEnumName<TSchema>() where TSchema : Schema, new();
}
public class EnumDbField<T>() : StructDbField<T>(EnumValidator<T>.Create(), GetSqlType()), IEnumDbField
    where T : struct, Enum
{
    public override string StringValue()
    {
        var value = Value() ?? throw new InvalidOperationException("Value is null");
        return value.ToString();
    }

    public override object ObjectValue()
    {
        var value = Value() ?? throw new InvalidOperationException("Value is null");
        return value;
    }

    private static string GetSqlType()
    {
        return $"ENUM ({string.Join(",", Enum.GetNames(typeof(T)).Select(name => $"'{name}'"))})";
    }
    
    public override void LoadValue(object? value)
    {
        if (value is string strValue)
        {
            foreach (T enumValue in Enum.GetValues(typeof(T)))
            {
                if (!strValue.Equals(enumValue.ToString())) continue;
                Value(enumValue);
                return;
            }
        }

        base.LoadValue(value);
    }

    public string GetSchemaEnumName<TSchema>() where TSchema : Schema, new()
    {
        Schema schema = Schema.Get<TSchema>();
        if (!schema.HasEnum<T>())
            throw new Exception($"Enum {typeof(T).Name} is not registered for schema {schema.Name}.");
        return schema.GetEnumName<T>();
    }

    internal override ConditionEx<TU> ParseEx<TU>(string op, object value) =>
        op switch
        {
            "=" => new EnumEx<TU>(Name, true, value.ToString()!),
            "!=" => new EnumEx<TU>(Name, false, value.ToString()!),
            _ => throw new NotSupportedException($"Operator {op} is not supported for enum fields.")
        };
}
internal record EnumEx<T>(string Name, bool IsEqual, string Value) : ConditionEx<T>(Name) where T : Model
{
    public override string ToQuery(out List<object> values)
    {
        var op = IsEqual ? "=" : "!=";
        values = [];
        return $"{GetName()} {op} '{Value}'";
    }
    
    public override bool Verify(T model)
    {
        var value = (string)GetValue(model);
        return IsEqual ? value == Value : value != Value;
    }
}