using ClaimHub.Domain.Enums;
using ClaimHub.Domain.Services;
using ClaimHub.Domain.Tests.Support;

namespace ClaimHub.Domain.Tests.Policies;

public class PolicyNumberGeneratorTests
{
    [Theory]
    [InlineData(ProductType.Home, "HOM-2025-000042")]
    [InlineData(ProductType.Motor, "MOT-2025-000042")]
    [InlineData(ProductType.Travel, "TRV-2025-000042")]
    public void Given_Product_When_Formatting_Then_UsesProductPrefix(ProductType product, string expected)
    {
        Assert.Equal(expected, PolicyNumberGenerator.Format(product, 2025, 42));
    }

    [Fact]
    public void Given_ZeroSequence_When_Formatting_Then_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => PolicyNumberGenerator.Format(ProductType.Home, 2025, 0));
    }

    [Fact]
    public void Given_SequenceValue_When_Generating_Then_ReturnsFormattedNumber()
    {
        var generator = new PolicyNumberGenerator(new FakePolicyNumberSequence(7));

        Assert.Equal("MOT-2025-000007", generator.Generate(ProductType.Motor, 2025));
    }

    [Fact]
    public void Given_Generator_When_Generating_Then_AsksSequenceOnce()
    {
        var sequence = new FakePolicyNumberSequence(3);
        var generator = new PolicyNumberGenerator(sequence);

        generator.Generate(ProductType.Home, 2025);

        Assert.Single(sequence.Calls);
        Assert.Equal((ProductType.Home, 2025), sequence.Calls[0]);
    }

    [Fact]
    public void Given_ValidNumber_When_Parsing_Then_ReturnsParts()
    {
        var parsed = PolicyNumberGenerator.TryParse("TRV-2024-000123", out var product, out var year, out var sequence);

        Assert.True(parsed);
        Assert.Equal(ProductType.Travel, product);
        Assert.Equal(2024, year);
        Assert.Equal(123, sequence);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("HOM-2025")]
    [InlineData("XXX-2025-000001")]
    public void Given_BadNumber_When_Parsing_Then_ReturnsFalse(string? number)
    {
        Assert.False(PolicyNumberGenerator.TryParse(number, out _, out _, out _));
    }
}
