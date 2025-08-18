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
}