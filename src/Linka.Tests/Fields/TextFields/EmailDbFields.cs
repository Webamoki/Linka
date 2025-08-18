using NUnit.Framework;
using Webamoki.Linka.Fields.TextFields;
using Webamoki.TestUtils;

namespace Linka.Tests.Fields.TextFields;

public class EmailDbFieldTest
{
    [Test]
    public void Validator_CachesCorrectly()
    {
        var validator1 = EmailValidator.Create();
        var validator2 = EmailValidator.Create();
        var validator3 = EmailValidator.Create(10, 100);

        Ensure.Equal(validator1, validator2);
        Ensure.NotEqual(validator1, validator3);
    }

    [Test]
    public void EmailValidator_ValidEmails_Pass()
    {
        var validator = EmailValidator.Create();
        Ensure.True(validator.IsValid("test@example.com", out _));
        Ensure.True(validator.IsValid("user.name+tag+sorting@example.co.uk", out _));
    }

    [Test]
    public void EmailValidator_InvalidEmails_Fail()
    {
        var validator = EmailValidator.Create();

        Ensure.False(validator.IsValid(null!, out _));
        Ensure.False(validator.IsValid("", out _));
        Ensure.False(validator.IsValid("plainaddress", out _));
        Ensure.False(validator.IsValid("missing-at.com", out _));
        Ensure.False(validator.IsValid("@missing-local.org", out _));
        Ensure.False(validator.IsValid("user@.com", out _));
        Ensure.False(validator.IsValid("user@com", out _)); 
        Ensure.False(validator.IsValid(12345, out _)); 
    }

    [Test]
    public void EmailValidator_LengthBounds_AreEnforced()
    {
        var validator = EmailValidator.Create(10, 20);

        Ensure.False(validator.IsValid("a@b.co", out _));
        Ensure.True(validator.IsValid("longer.email@ok.com", out _));
        Ensure.False(validator.IsValid("a".PadRight(50, 'a') + "@test.com", out _));
    }

    [Test]
    public void ChangesVerifyCorrectly()
    {
        var field = new EmailDbField();
        Ensure.True(field.IsEmpty());
        Ensure.Equal("VARCHAR(50)", field.SQLType);
        field.Value("test@gmail.com");
        Ensure.False(field.IsEmpty());
        Ensure.True(field.IsSet);
        Ensure.False(field.IsChanged());
        field.Value(null);
        Ensure.True(field.IsEmpty());
        Ensure.True(field.IsSet);
        Ensure.True(field.IsChanged());
        field.ResetChange();
        Ensure.False(field.IsChanged());
        Ensure.Equal(null, field.Value());
        field.Value("asd@gmail.com");
        Ensure.True(field.IsChanged());
    }
}