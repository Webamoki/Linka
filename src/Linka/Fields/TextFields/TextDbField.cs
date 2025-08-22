using System.Text.RegularExpressions;
using Webamoki.Utils;

namespace Webamoki.Linka.Fields.TextFields;

public class TextValidator : Validator
{
    private readonly int _maxLength;
    private readonly int _minLength;
    private readonly Regex? _regex;

    protected TextValidator(int minLength, int maxLength, string? regexPattern = null, bool isInjectable = true)
    {
        if (minLength < 1) throw new ArgumentOutOfRangeException(nameof(minLength), "Min length cannot be negative");
        if (maxLength < 1) throw new ArgumentOutOfRangeException(nameof(maxLength), "Max length cannot be negative");
        if (maxLength < minLength)
            throw new ArgumentOutOfRangeException(nameof(maxLength), "Max length cannot be less than min length");

        _minLength = minLength;
        _maxLength = maxLength;
        IsInjectable = isInjectable;
        _regex = string.IsNullOrWhiteSpace(regexPattern) ? null : new Regex(regexPattern, RegexOptions.Compiled);
    }

    public static TextValidator Create(int minLength = 1, int maxLength = 255, string? regexPattern = null,
        bool isInjectable = true)
    {
        var regexHash = regexPattern ?? "null";
        var hash = $"{minLength}:{maxLength}:{regexHash}:{isInjectable}";
        if (Load<TextValidator>(hash, out var validator))
            return validator!;

        validator = new TextValidator(minLength, maxLength, regexPattern, isInjectable);
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

        if (s.Length > _maxLength || s.Length < _minLength)
        {
            message = $"Value length is not in range [{_minLength}, {_maxLength}]";
            return false;
        }

        if (_regex != null)
        {
            if (!_regex.IsMatch(s))
            {
                message = $"Value does not match regex {_regex}";
                return false;
            }

            if (ValueValidations.HasBannedCharacters(s))
            {
                message = $"Value matches regex {_regex}";
                return false;
            }
        }

        message = null;
        return true;
    }
}

public class TextDbField(Validator validator, int charSize)
    : RefDbField<string>(validator, $"VARCHAR({charSize})")
{
    public TextDbField(int maxLength = 255, int minLength = 1) : this(TextValidator.Create(minLength), maxLength) { }
    public override string StringValue() => Value() ?? throw new InvalidOperationException("Value is null");

    public override object ObjectValue()
    {
        var value = Value() ?? throw new InvalidOperationException("Value is null");
        return value;
    }
}

public class NameDbField() : TextDbField(50);