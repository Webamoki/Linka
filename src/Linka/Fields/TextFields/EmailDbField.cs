using System.Text.RegularExpressions;

namespace Webamoki.Linka.Fields.TextFields;

using System.Net.Mail;

public partial class EmailValidator : TextValidator
{
    private EmailValidator(int minLength, int maxLength) : base(minLength, maxLength) { }

    public static EmailValidator Create(int minLength = 5, int maxLength = 255)
    {
        var hash = $"{minLength}:{maxLength}";
        if (Load<EmailValidator>(hash, out var validator))
            return validator!;
        validator = new EmailValidator(minLength, maxLength);
        Register(hash, validator);
        return validator;
    }

    public override bool IsValid(object? value, out string? message)
    {
        if (!base.IsValid(value, out message)) return false;

        try
        {
            var str = (value as string)!;
            var addr = new MailAddress(str);
            if (addr.Address != str) return false;

            // Enforce stricter domain rule: must contain at least one dot
            var domain = addr.Host;
            if (MyRegex().IsMatch(domain))
            {
                message = null;
                return true;
            }
        }
        catch
        {
            
        }
        message = "Value is not a valid email address";
        return false;
    }

    [GeneratedRegex(@"^[^@\s]+\.[^@\s]+$")]
    private static partial Regex MyRegex();
}
public class EmailDbField(int maxLength = 50) : TextDbField(EmailValidator.Create(), maxLength) { }
