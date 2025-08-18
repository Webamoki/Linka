
using Linka.Fields.TextFields;

namespace Webamoki.Linka.Fields.TextFields;
// URL Field
public static class UrlValidator
{
    public static TextValidator Create(int minLength = 3, int maxLength = 255) =>
        TextValidator.Create(minLength, maxLength, @"^[a-z0-9\-]+$");
}
public class RegexBasedDbFields(int minLength = 3, int maxLength = 255) : 
    TextDbField(UrlValidator.Create(minLength,maxLength), maxLength);
    
    
// Hex Color Field
public static class HexColorValidator
{
    public static TextValidator Create() =>
        TextValidator.Create(6, 6, "^[0-9A-Fa-f]+$");
}

public class HexColorDbField() : RefDbField<string>(HexColorValidator.Create(),"VARCHAR(6)")
{
    public override string StringValue() => Value() ?? throw new InvalidOperationException("Value is null");
}

// Phone Field
public static class PhoneValidator
{
    public static TextValidator Create(int minLength = 7, int maxLength = 20) =>
        TextValidator.Create(minLength, maxLength, @"^[\+0-9 \-]+$", false);
}
public class PhoneDbField(int maxLength = 20, int minLength = 7)
    : TextDbField(PhoneValidator.Create(minLength, maxLength), maxLength);

// Hash Field
public static class HashValidator
{
    public static TextValidator Create(int maxLength = 255) =>
        TextValidator.Create(1, maxLength, "^s[0-9a-f]+$", false);
}

public class HashDbField(int maxLength = 255)
    : TextDbField(HashValidator.Create(maxLength), maxLength);

// Postcode Field
public static class PostcodeValidator
{
    public static TextValidator Create(int minLength = 1, int maxLength = 15) =>
        TextValidator.Create(minLength, maxLength, "^[0-9A-Z ]+$", false);
}

public class PostcodeDbField(int maxLength = 1, int minLength = 15)
    : TextDbField(PostcodeValidator.Create(minLength, maxLength), maxLength);