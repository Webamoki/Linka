using NUnit.Framework;
using Webamoki.Linka.Fields;
using Webamoki.Utils.Testing;

namespace Tests.Fields;

public class IdDbFieldTest
{
    private const string DefaultPool = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    [Test]
    public void Validator_CachesCorrectly()
    {
        var v1 = IdValidator.Create(8, DefaultPool);
        var v2 = IdValidator.Create(8, DefaultPool);
        Ensure.Equal(v1, v2);
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void IdValidator_InvalidLength_Throws(int length) => Assert.Throws<ArgumentOutOfRangeException>(() => _ = IdValidator.Create(length, DefaultPool));

    [TestCase("ABAB", 4, "AB", true)]
    [TestCase("ABA", 4, "AB", false)]
    [TestCase("ABCX", 4, "AB", false)]
    [TestCase("1234", 4, "AB", false)]
    public void IdValidator_IsValid_WorksCorrectly(string value, int length, string pool, bool expected)
    {
        var validator = IdValidator.Create(length, pool);
        Ensure.Equal(expected, validator.IsValid(value, out _));
    }

    [TestCase(1234)]
    [TestCase(null)]
    [TestCase(12.34)]
    public void IdValidator_IsValid_ReturnsFalseForNonString(object input)
    {
        var validator = IdValidator.Create(4, "AB");
        Ensure.False(validator.IsValid(input, out _));
    }

    [Test]
    public void IdValidator_GenerateValue_CreatesValidValue()
    {
        var validator = IdValidator.Create(6, "XYZ");
        var value = validator.GenerateValue();
        Ensure.Equal(6, value.Length);
        foreach (var c in value)
            Ensure.True("XYZ".Contains(c));
    }

    [Test]
    public void IdDbField_Constructs_Correctly()
    {
        var field = new IdDbField();
        Ensure.Equal("VARCHAR(10)", field.SQLType);
        Ensure.True(field.IsEmpty);
    }

    [Test]
    public void IdDbField_StringValue_ReturnsEmptyIfUnset()
    {
        var field = new IdDbField();
        Ensure.Equal(string.Empty, field.StringValue());
    }

    [TestCase("ABCDEF1234")]
    [TestCase("XYZ9998888")]
    public void IdDbField_StringValue_ReturnsSetValue(string value)
    {
        var field = new IdDbField();
        field.Value(value);
        Ensure.Equal(value, field.StringValue());
    }

    [TestCase("ABCDEF1234")]
    [TestCase("XYZ9998888")]
    public void IdDbField_ObjectValue_ReturnsSetValue(string value)
    {
        var field = new IdDbField();
        field.Value(value);
        Ensure.Equal(value, field.ObjectValue());
    }

    [TestCase("ABCDE")]
    public void ShortIdDbField_HasCorrectLength(string value)
    {
        var field = new ShortIdDbField();
        field.Value(value);
        Ensure.Equal(value, field.StringValue());
    }

    [TestCase("Ab1Zx")]
    [TestCase("X9Y8Z")]
    public void TokenDbField_AllowsAlphanumericCharacters(string value)
    {
        var field = new TokenDbField();
        field.Value(value);
        Ensure.Equal(value, field.StringValue());
    }

    [Test]
    public void ChangesVerifyCorrectly()
    {
        var field = new ShortIdDbField();
        Ensure.True(field.IsEmpty);
        Ensure.Equal("VARCHAR(5)", field.SQLType);
        field.Value("AAAAA");
        Ensure.False(field.IsEmpty);
        Ensure.True(field.IsValid(out _));
        field.Value(null);
        Ensure.True(field.IsEmpty);
        Ensure.Equal(null, field.Value());
        field.GenerateValue();
        Ensure.True(field.IsValid(out _));
    }
}