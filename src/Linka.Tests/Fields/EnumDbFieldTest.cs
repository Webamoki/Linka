using NUnit.Framework;
using Webamoki.Linka.Fields;
using Webamoki.Utils.Testing;

namespace Tests.Fields;

public enum TestEnum
{
    Alpha,
    Beta,
    Gamma
}

public class EnumValidatorTest
{
    [Test]
    public void Validator_CachesCorrectly()
    {
        var validator1 = EnumValidator<TestEnum>.Create();
        var validator2 = EnumValidator<TestEnum>.Create();
        Ensure.Equal(validator1, validator2);
    }


    [TestCase(TestEnum.Alpha)]
    [TestCase("Alpha")]
    [TestCase(TestEnum.Gamma)]
    public void EnumValidator_ValidEnumValues_AreValidatedCorrectly(object input)
    {
        var validator = EnumValidator<TestEnum>.Create();
        Ensure.True(validator.IsValid(input, out _));
    }

    [TestCase(123)]
    [TestCase(null)]
    [TestCase((TestEnum)999)] // Undefined enum value
    public void EnumValidator_InvalidTypes_AreRejected(object input)
    {
        var validator = EnumValidator<TestEnum>.Create();
        Ensure.False(validator.IsValid(input, out _));
    }

    [Test]
    public void ChangesVerifyCorrectly()
    {
        var field = new EnumDbField<TestEnum>();
        field.Value(TestEnum.Alpha);
        Ensure.False(field.IsEmpty);
        field.Value(null);
        Ensure.True(field.IsEmpty);
        Ensure.Equal(null, field.Value());
        field.Value(TestEnum.Beta);
    }

    [Test]
    public void EnumDbField_StringValue_ReturnsCorrectEnumName()
    {
        var field = new EnumDbField<TestEnum>();
        Ensure.True(field.IsEmpty);
        field.Value(TestEnum.Beta);
        Ensure.Equal("Beta", field.StringValue());
    }

    [Test]
    public void EnumDbField_ObjectValue_ReturnsCorrectEnumValue()
    {
        var field = new EnumDbField<TestEnum>();
        Ensure.True(field.IsEmpty);
        field.Value(TestEnum.Beta);
        Ensure.Equal(TestEnum.Beta, field.ObjectValue());
    }

    [Test]
    public void EnumDbField_ObjectValue_ThrowsIfUnset()
    {
        var field = new EnumDbField<TestEnum>();
        Ensure.True(field.IsEmpty);
        Ensure.Throws<InvalidOperationException>(() => _ = field.ObjectValue());
    }

    [Test]
    public void EnumDbField_StringValue_ThrowsWhenUnset()
    {
        var field = new EnumDbField<TestEnum>();
        Ensure.Throws<Exception>(() => field.StringValue());
    }
}