using NUnit.Framework;
using Webamoki.Linka.Fields.TextFields;
using Webamoki.TestUtils;

namespace Linka.Tests.Fields.TextFields;

public class RegexValidatorTest
{
    [TestCase("ffffff")]
    [TestCase("000000")]
    [TestCase("a1b2c3")]
    [TestCase("ABCDEF")]
    [TestCase("123abc")]
    public void HexColorValidator_ValidValues(string input)
    {
        var validator = HexColorValidator.Create();
        Ensure.True(validator.IsValid(input, out _));
    }

    [TestCase("#ffffff")]
    [TestCase("fff")]
    [TestCase("gggggg")]
    [TestCase("12345")]
    [TestCase("1234567")]
    [TestCase("")]
    [TestCase(null)]
    public void HexColorValidator_InvalidValues(object? input)
    {
        var validator = HexColorValidator.Create();
        Ensure.False(validator.IsValid(input, out _));
    }

    [TestCase("0123456789")]
    [TestCase("+44 1234 567890")]
    [TestCase("0033-123-4567")]
    [TestCase("0800 123 456")]
    public void PhoneValidator_ValidValues(string input)
    {
        var validator = PhoneValidator.Create();
        Ensure.True(validator.IsValid(input, out _));
    }

    [TestCase("call me maybe")]
    [TestCase("(123) 456-7890")] // parentheses not allowed
    [TestCase("123-456-7890x123")] // extension not allowed
    [TestCase("1234567890#")] // symbols like # not allowed
    [TestCase(1234567890)] // not a string
    [TestCase(null)]
    [TestCase("")]
    public void PhoneValidator_InvalidValues(object input)
    {
        var validator = PhoneValidator.Create();
        Ensure.False(validator.IsValid(input, out _));
    }

    [TestCase("abc", true)]
    [TestCase("abc-123", true)]
    [TestCase("a-b-c", true)]
    [TestCase("12345", true)]
    public void UrlValidator_ValidValues(string url, bool expectedResult)
    {
        var validator = UrlValidator.Create();
        Ensure.Equal(validator.IsValid(url, out _), expectedResult);
    }

    [TestCase(null, false)]
    [TestCase("", false)]
    [TestCase("abc_def", false)] // underscore not allowed
    [TestCase("ABC", false)] // uppercase not allowed
    [TestCase("abc!", false)] // special character not allowed
    [TestCase(12345, false)] // wrong type
    public void UrlValidator_InvalidValues(object url, bool expectedResult)
    {
        var validator = UrlValidator.Create();
        Ensure.Equal(validator.IsValid(url, out _), expectedResult);
    }

    [TestCase("s1a2b3c", true)]
    [TestCase("sabcdef", true)]
    [TestCase("s1234567890abcdef", true)]
    [TestCase("s0", true)]
    public void HashValidator_ValidValues_Pass(string value, bool expected)
    {
        var validator = HashValidator.Create();
        Ensure.Equal(validator.IsValid(value, out _), expected);
    }

    [TestCase(null, false)]
    [TestCase("", false)] // too short
    [TestCase("abcdef", false)] // missing starting 's'
    [TestCase("S123abc", false)] // capital 'S' not allowed
    [TestCase("s123XYZ", false)] // uppercase letters not allowed
    [TestCase("s123-456", false)] // special characters not allowed
    [TestCase(12345, false)] // wrong type
    public void HashValidator_InvalidValues_Fail(object value, bool expected)
    {
        var validator = HashValidator.Create();
        Ensure.Equal(validator.IsValid(value, out _), expected);
    }

    [TestCase("SW1A 1AA", true)]
    [TestCase("EC1A 1BB", true)]
    [TestCase("W1A 0AX", true)]
    [TestCase("M1 1AE", true)]
    [TestCase("B33 8TH", true)]
    [TestCase("CR2 6XH", true)]
    [TestCase("DN55 1PT", true)]
    [TestCase("12345", true)] // numbers allowed
    [TestCase("A1 B2", true)] // letters and spaces
    public void PostcodeValidator_ValidValues_Pass(string value, bool expected)
    {
        var validator = PostcodeValidator.Create();
        Ensure.Equal(validator.IsValid(value, out _), expected);
    }

    [TestCase("", false)]
    [TestCase(null, false)]
    [TestCase("sw1a 1aa", false)] // lowercase not allowed
    [TestCase("SW1A_1AA", false)] // underscore not allowed
    [TestCase("SW1A-1AA", false)] // dash not allowed
    [TestCase("123@456", false)] // special character not allowed
    [TestCase(12345, false)] // wrong type
    public void PostcodeValidator_InvalidValues_Fail(object value, bool expected)
    {
        var validator = PostcodeValidator.Create();
        Ensure.Equal(validator.IsValid(value, out _), expected);
    }
}