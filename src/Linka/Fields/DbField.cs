using Webamoki.Linka.Expressions.Ex;
using Webamoki.Linka.ModelSystem;

namespace Webamoki.Linka.Fields;

public abstract class DbField(
    Validator validator,
    string sqlType)
{
    public bool IsPrimary { get; internal set; }
    public bool IsUnique { get; internal set; }
    public int Search { get; internal set; }
    public bool IsRequired { get; internal set; } = true;
    public bool IsSet { get; protected set; }

    /// <summary>
    ///     Returns the SQL type of the field, e.g., varchar, int, tinyint, datetime, etc.
    /// </summary>
    /// <value>The SQL type as a string.</value>
    public string SQLType { get; } = sqlType;

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
    public abstract object ObjectValue();

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

    public abstract void LoadValue(object? value);
    public abstract bool IsChanged();
    public abstract void ResetChange();
    public abstract bool IsValid(out string? message);
    
    internal abstract Ex<T> ParseEx<T>(string op, object value) where T : Model;
}

public abstract class RefDbField<T>(
    Validator validator,
    string sqlType)
    : DbField(validator, sqlType)
{
    private T? _value;
    private T? _oldValue;

    public override void LoadValue(object? value)
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

    public void Value(T? value)
    {
        if (!IsSet)
        {
            if (value != null)
            {
                _value = value;
                _oldValue = value;
            }

            IsSet = true;
        }
        else
        {
            _value = value;
        }
    }

    public override bool IsChanged() =>
        !EqualityComparer<T>.Default.Equals(_value, _oldValue);

    public override void ResetChange() => _oldValue = _value;


    public override bool IsValid(out string? message)
    {
        if (!IsSet) throw new InvalidOperationException();
        return IsValid(_value, out message);
    }

    public bool IsEmpty() => _value == null;
    public virtual T? Value() => _value;

    public override object? ObjectValue() => _value;

    public static bool operator ==(RefDbField<T> left, T? right)
    {
        if (left.IsEmpty() && right == null) return true;
        if (left.IsEmpty() || right == null) return false;
        return left.Value()!.Equals(right);
    }

    public static bool operator !=(RefDbField<T> left, T? right) => !(left == right);
    
    internal override Ex<TU> ParseEx<TU>(string op, object value)
    {
        if (value is not T t)
            throw new InvalidCastException($"Value is not of type {typeof(T).Name}");
        
        if (Validator.IsInjectable)
        {
            return op switch
            {
                "=" => new EqEx<TU>(Name, new ObjectValueEx<TU>(t)),
                "!=" => new NeqEx<TU>(Name, new ObjectValueEx<TU>(t)),
                _ => throw new NotSupportedException($"Operator {op} is not supported for field {Name}")
            };
        }
        return op switch
        {
            "=" => new EqEx<U>(Name, t),
            "!=" => new NeqEx<U>(Name, t),
            _ => throw new NotSupportedException($"Operator {op} is not supported for field {Name}")
        };
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
    private T? _oldValue;

    public virtual void Value(T? value)
    {
        if (!IsSet)
        {
            if (value != null)
            {
                _value = value;
                _oldValue = value;
            }

            IsSet = true;
        }
        else
        {
            _value = value;
        }
    }

    public override void LoadValue(object? value)
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

    public override bool IsChanged() => !Equals(_value, _oldValue);

    public override void ResetChange() => _oldValue = _value;

    public override bool IsValid(out string? message)
    {
        if (!IsSet) throw new InvalidOperationException();
        return IsValid(_value, out message);
    }

    public bool IsEmpty() => _value == null;
    public virtual T? Value() => _value;

    public override object? ObjectValue() => _value;

    public static bool operator ==(StructDbField<T> left, T? right)
    {
        if (left.IsEmpty() && right == null) return true;
        if (left.IsEmpty() || right == null) return false;
        return left.Value()!.Equals(right);
    }

    public static bool operator !=(StructDbField<T> left, T? right) => !(left == right);
}