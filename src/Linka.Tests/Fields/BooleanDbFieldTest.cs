using NUnit.Framework;
using Webamoki.Linka.Fields;
using Webamoki.Utils.Testing;

namespace Tests.Fields;

public class BooleanDbFieldTest
{
    [Test]
    public void Validator_CachesCorrectly()
    {
        var v1 = BooleanValueValidator.Create();
        var v2 = BooleanValueValidator.Create();
        Ensure.Equal(v1, v2);
    }

    [TestCase(true, true)]
    [TestCase(false, true)]
    [TestCase("true", false)]
    [TestCase(1, false)]
    [TestCase(null, false)]
    public void Validator_ValidatesOnlyBooleans(object input, bool expected)
    {
        var validator = BooleanValueValidator.Create();
        Ensure.Equal(expected, validator.IsValid(input, out _));
    }


    [Test]
    public void BooleanDbField_StringValue_True()
    {
        var field = new BooleanDbField();
        field.Value(true);
        Ensure.Equal("true", field.StringValue());
    }

    [Test]
    public void BooleanDbField_StringValue_False()
    {
        var field = new BooleanDbField();
        field.Value(false);
        Ensure.Equal("false", field.StringValue());
    }

    [Test]
    public void BooleanDbField_ObjectValue_True()
    {
        var field = new BooleanDbField();
        field.Value(true);
        Ensure.Equal(true, field.ObjectValue());
    }

    [Test]
    public void BooleanDbField_ObjectValue_False()
    {
        var field = new BooleanDbField();
        field.Value(false);
        Ensure.Equal(false, field.ObjectValue());
    }

    [Test]
    public void BooleanDbField_ObjectValue_ThrowsIfUnset()
    {
        var field = new BooleanDbField();
        Ensure.True(field.IsEmpty());
        Ensure.Throws<InvalidOperationException>(() => _ = field.ObjectValue());
    }

    [Test]
    public void BooleanDbField_StringValue_ThrowsIfUnset()
    {
        var field = new BooleanDbField();
        Assert.Throws<InvalidOperationException>(() => _ = field.StringValue());
    }

    [Test]
    public void ChangesVerifyCorrectly()
    {
        var field = new BooleanDbField();
        Ensure.True(field.IsEmpty());
        Ensure.Equal("BOOLEAN", field.SQLType);
        field.Value(true);
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
        field.Value(false);
        Ensure.True(field.IsChanged());
    }
}