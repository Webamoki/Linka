using System.Text;

namespace Webamoki.Linka.Fields;


public class IdValidator : Validator
{
    private readonly int _length;
    private readonly HashSet<char> _pool;
    private readonly char[] _poolArray;

    private IdValidator(int length, string pool)
    {
        if (length < 1) throw new ArgumentOutOfRangeException(nameof(length), "Length cannot be negative");
        _length = length;
        _pool = new HashSet<char>(pool);
        _poolArray = pool.ToCharArray();
        IsInjectable = false;
    }

    public static IdValidator Create(int length, string pool)
    {
        var hash = $"{pool}:{length}";
        if (Load<IdValidator>(hash, out var validator))
            return validator!;
        validator = new IdValidator(length, pool);
        Register(hash, validator);
        return validator;
    }
    public override bool IsValid(object? value, out string? message)
    {
        if (value is not string s)
        {
            message = "Value is not a string";
            return false;
        }

        if (s.Length != _length)
        {
            message = $"Value length is not {_length}";
            return false;
        }

        foreach (var c in s)
        {
            if (_pool.Contains(c)) continue;
            message = $"Value contains invalid character {c}";
            return false;
        }
        message = null;
        return true;
    }

    public string GenerateValue()
    {
        var sb = new StringBuilder(_length);
        var rng = Random.Shared;

        for (var i = 0; i < _length; i++)
        {
            var index = rng.Next(_poolArray.Length);
            sb.Append(_poolArray[index]);
        }

        return sb.ToString();
    }
}


public class IdDbField(Validator validator, int charSize)
    : RefDbField<string>(validator, $"VARCHAR({charSize})")
{
    protected IdDbField(int length, string pool = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789") : this(IdValidator.Create(length, pool), length) { }
    public IdDbField() : this(10) { }
    public override string StringValue() => Value() ?? string.Empty;
    
    public void GenerateValue() => Value(((IdValidator)Validator).GenerateValue());
}


public class ShortIdDbField() : IdDbField(5);
public class TokenDbField() : IdDbField(5, "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789");
public class CountryCodeDbField() : IdDbField(2, "ABCDEFGHIJKLMNOPQRSTUVWXYZ");
