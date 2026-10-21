using ClaimHub.Domain.Rules;
using FluentAssertions;

namespace ClaimHub.Domain.Tests.Validation;

public class NationalNumberValidatorTests
{
    [Theory]
    [InlineData("85.07.15-123.45")]
    [InlineData("00.12.31-001.01")]
    public void Given_ExpectedFormat_When_Validating_Then_ReturnsTrue(string value)
    {
        NationalNumberValidator.IsValidFormat(value).Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("85071512345")]
    [InlineData("85.13.15-123.45")]
    [InlineData("85.07.32-123.45")]
    public void Given_WrongFormat_When_Validating_Then_ReturnsFalse(string? value)
    {
        NationalNumberValidator.IsValidFormat(value).Should().BeFalse();
    }

    [Fact]
    public void Given_Number_When_Masking_Then_KeepsOnlyEdges()
    {
        NationalNumberValidator.Mask("85.07.15-123.45").Should().Be("85.*********.45");
    }
}
