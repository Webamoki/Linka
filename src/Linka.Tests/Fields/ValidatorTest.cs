using NUnit.Framework;
using Webamoki.Linka.Fields.TextFields;
using Webamoki.TestUtils;

namespace LinkaTests.Fields;

public class ValidatorTest
{
    [Test]
    public void Construct_CheckReference()
    {
        var validator = TextValidator.Create();
        var validator1 = TextValidator.Create();
        var validator2 = TextValidator.Create(2);
        var validator3 = TextValidator.Create(2);
        Ensure.Equal(validator, validator1);
        Ensure.Equal(validator2, validator3);
        Ensure.NotEqual(validator1, validator3);
    }
    
    [Test]
    public void TextValidator_CheckValues()
    {
        var validator1 = TextValidator.Create();
        var validator2 = TextValidator.Create(2);
        Ensure.False(validator1.IsValid(true, out _));
        Ensure.True(validator1.IsValid("asd", out _));
        Ensure.False(validator2.IsValid("a", out _));
        Ensure.True(validator2.IsValid("a213", out _));
    }
}