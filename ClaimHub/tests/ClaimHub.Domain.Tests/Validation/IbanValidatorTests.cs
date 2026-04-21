using ClaimHub.Domain.Rules;

namespace ClaimHub.Domain.Tests.Validation;

public class IbanValidatorTests
{
    [Theory]
    [InlineData("BE68539007547034")]
    [InlineData("NL91ABNA0417164300")]
    public void Should_Accept_Well_Formed_Iban(string iban)
    {
        Assert.True(IbanValidator.IsValid(iban));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("be68539007547034")]
    [InlineData("BE6853")]
    public void Should_Reject_Malformed_Iban(string? iban)
    {
        Assert.False(IbanValidator.IsValid(iban));
    }

    [Fact]
    public void Should_Recognise_Belgian_Iban()
    {
        Assert.True(IbanValidator.IsBelgian("BE68539007547034"));
        Assert.False(IbanValidator.IsBelgian("NL91ABNA0417164300"));
    }

    [Fact]
    public void Should_Mask_Middle_Of_Iban()
    {
        Assert.Equal("BE68********7034", IbanValidator.Mask("BE68539007547034"));
    }

    [Fact]
    public void Should_Fully_Mask_Short_Values()
    {
        Assert.Equal("****", IbanValidator.Mask("BE12"));
    }
}
