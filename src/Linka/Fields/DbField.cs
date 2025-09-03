using Webamoki.Linka.Expressions;
using Webamoki.Linka.ModelSystem;
using Webamoki.Linka.SchemaSystem;

namespace Webamoki.Linka.Fields;

public abstract class DbField(
    Validator validator,
    string sqlType)
{
    
    internal bool IsPrimary { get; set; }
    internal bool IsUnique { get; set; }
    internal int Search { get; set; }
    internal bool IsRequired { get;  set; } = true;
    /// <summary>
    ///     Returns the SQL type of the field, e.g., varchar, int, tinyint, datetime, etc.
    /// </summary>
    /// <value>The SQL type as a string.</value>
    internal string SQLType { get; } = sqlType;

    public Validator Validator { get; } = validator;

    private string? _name;
    public string Name => _name ?? throw new InvalidOperationException("Name has not been set.");

    internal void SetName(string name)
    {
        if (_name != null)
            throw new InvalidOperationException("Name can only be set once.");
        _name = name;
    }

    public void ApplyAttributes(object[] attributes)
    {
        foreach (var attribute in attributes)
            switch (attribute)
            {
                case NotRequiredAttribute:
                    IsRequired = false;
                    break;
                case KeyAttribute:
                    IsPrimary = true;
                    break;
                case UniqueAttribute:
                    IsUnique = true;
                    break;
                case FullTextAttribute search:
                    Search = search.Weight;
                    break;
            }
    }

    public abstract string StringValue();
    internal abstract object ObjectValue();
    internal abstract void Value(object? value);
    public virtual bool IsEmpty => true;

    public bool IsValid(object? value, out string? message)
    {
        if (value == null)
        {
            if (IsRequired)
            {
                message = "Value cannot be null";
                return false;
            }

            message = null;
            return true;
        }

        return Validator.IsValid(value ?? throw new InvalidOperationException(), out message);
    }

    public abstract bool IsValid(out string? message);

    internal abstract ConditionEx<T> ParseEx<T>(string op, object value) where T : Model;
    internal abstract string GetUpdateSetQuery<TSchema>(object value, out object? queryValue) where TSchema : Schema, new();
}

public abstract class RefDbField<T>(
    Validator validator,
    string sqlType)
    : DbField(validator, sqlType)
{
    private T? _value;

    internal override void Value(object? value)
    {
        switch (value)
        {
            case T t:
                Value(t);
                break;
            case null:
                Value(default);
                break;
            default:
                throw new InvalidCastException($"Value is not of type {typeof(T).Name}");
        }
    }

    public void Value(T? value) => _value = value;

    public override bool IsValid(out string? message)
    {
        if (IsEmpty) throw new InvalidOperationException();
        return IsValid(_value, out message);
    }

    public override bool IsEmpty => _value == null;
    public T? Value() => _value;

    internal override object ObjectValue() => _value ?? throw new InvalidOperationException("Value is null");

    public static bool operator ==(RefDbField<T> left, T? right)
    {
        if (left.IsEmpty && right == null) return true;
        if (left.IsEmpty || right == null) return false;
        return left.Value()!.Equals(right);
    }

    public static bool operator !=(RefDbField<T> left, T? right) => !(left == right);
    internal override ConditionEx<TU> ParseEx<TU>(string op, object value)
    {
        return op switch
        {
            "=" => new StringEx<TU>(Name, true, (string)value, Validator.IsInjectable),
            "!=" => new StringEx<TU>(Name, false, (string)value, Validator.IsInjectable),
            _ => throw new NotSupportedException($"Operator {op} is not supported for field {Name}")
        };
    }
    internal override string GetUpdateSetQuery<TSchema>(object value, out object? queryValue)
    {
        if (Validator.IsInjectable)
        {
            queryValue = value;
            return "?";
        }
        queryValue = null;
        return $"'{value}'";
    }
}

/// <summary>
/// Same as AbstractDbField, but for structs like Boolean and Enum
/// </summary>
public abstract class StructDbField<T>(
    Validator validator,
    string sqlType)
    : DbField(validator, sqlType)
    where T : struct
{
    private T? _value;

    public void Value(T? value) => _value = value;

    internal override void Value(object? value)
    {
        switch (value)
        {
            case T t:
                Value(t);
                break;
            case null:
                Value(null);
                break;
            default:
                throw new InvalidCastException($"Value is not of type {typeof(T).Name}");
        }
    }
    public override bool IsValid(out string? message)
    {
        if (IsEmpty) throw new InvalidOperationException();
        return IsValid(_value, out message);
    }

    public override bool IsEmpty => _value == null;
    public T? Value() => _value;

    internal override object ObjectValue() => _value ?? throw new InvalidOperationException("Value is null");

    public static bool operator ==(StructDbField<T> left, T? right)
    {
        if (left.IsEmpty && right == null) return true;
        if (left.IsEmpty || right == null) return false;
        return left.Value()!.Equals(right);
    }

    public static bool operator !=(StructDbField<T> left, T? right) => !(left == right);


}