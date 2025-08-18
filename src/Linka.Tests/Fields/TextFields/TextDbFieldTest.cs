using NUnit.Framework;
using Webamoki.Linka.Fields.TextFields;
using Webamoki.TestUtils;

namespace LinkaTests.Fields.TextFields;

public class TextDbFieldTest
{
    [Test]
    public void Validator_CachesCorrectly()
    {
        var v1 = TextValidator.Create();
        var v2 = TextValidator.Create();
        var v3 = TextValidator.Create(10, 15);

        Ensure.Equal(v1, v2);
        Ensure.NotEqual(v1, v3);
    }

    
    [TestCase("hello", true)]
    [TestCase("", false)]
    [TestCase(null, false)]
    [TestCase(123, false)] // not a string
    public void CreateValidator_DefaultRange_ValidatesProperly(object input, bool expected)
    {
        var validator = TextValidator.Create();
        Ensure.Equal(expected, validator.IsValid(input, out _));
    }
    
    
    [TestCase("hi", true)]         // within 2-5
    [TestCase("hello", true)]      // boundary max
    [TestCase("h", false)]         // below min
    [TestCase("too long", false)]  // above max

    [TestCase("he", true)]         // exactly min
    [TestCase("hell", true)]       // just under max
    [TestCase("toolong", false)]   // still too long
    [TestCase("a", false)]         // still too short
    public void CreateValidator_CustomRange_ValidatesProperly(object input, bool expected)
    {
        var validator = TextValidator.Create(2, 5);
        Ensure.Equal(expected, validator.IsValid(input, out _));
    }


    [Test]
    public void Constructor_InvalidMinLength_Throws()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => TextValidator.Create(-1));
        Ensure.True(ex!.ParamName == "minLength");
    }
    
    [Test]
    public void ChangesVerifyCorrectly()
    {
        var field = new TextDbField(100, 10);
        Ensure.True(field.IsEmpty());
        Ensure.Equal("VARCHAR(100)", field.SQLType);
        field.Value("hello");
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
        field.Value("hello2");
        Ensure.True(field.IsChanged());
    }

    [Test]
    public void NameDbField_ConstructsCorrectly()
    {
        var nameField = new NameDbField();
        Ensure.True(nameField.IsEmpty());
        Ensure.Equal("VARCHAR(50)", nameField.SQLType);
    }
}