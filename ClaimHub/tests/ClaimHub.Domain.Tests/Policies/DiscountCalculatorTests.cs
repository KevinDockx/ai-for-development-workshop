using ClaimHub.Domain.Models;
using ClaimHub.Domain.Services;

namespace ClaimHub.Domain.Tests.Policies;

public class DiscountCalculatorTests
{
    private readonly DiscountCalculator _calculator = new();

    private static double RateOf(DiscountBreakdown breakdown, string name) =>
        breakdown.Lines.Single(l => l.Name == name).Rate;

    [Fact]
    public void Given_ThreeClaimFreeYears_When_Calculating_Then_NoClaimsBonusIs15Percent()
    {
        var breakdown = _calculator.Calculate(new PremiumCalculationRequest { ClaimFreeYears = 3 });

        Assert.Equal(0.15, RateOf(breakdown, "NoClaims"), 5);
    }

    [Fact]
    public void Given_TwoActivePolicies_When_Calculating_Then_MultiPolicyDiscountIs10Percent()
    {
        var breakdown = _calculator.Calculate(new PremiumCalculationRequest { ActivePolicyCount = 2 });

        Assert.Equal(0.10, RateOf(breakdown, "MultiPolicy"), 5);
    }

    [Fact]
    public void Given_OnlyOneActivePolicy_When_Calculating_Then_NoMultiPolicyDiscount()
    {
        var breakdown = _calculator.Calculate(new PremiumCalculationRequest { ActivePolicyCount = 1 });

        Assert.Equal(0, RateOf(breakdown, "MultiPolicy"));
    }

    [Theory]
    [InlineData(4, 0)]
    [InlineData(5, 0.02)]
    [InlineData(10, 0.04)]
    [InlineData(15, 0.06)]
    public void Given_YearsAsCustomer_When_Calculating_Then_LoyaltyGrowsPerFiveYears(int years, double expected)
    {
        var breakdown = _calculator.Calculate(new PremiumCalculationRequest { FullYearsAsCustomer = years });

        Assert.Equal(expected, RateOf(breakdown, "Loyalty"), 5);
    }

    [Fact]
    public void Given_AllDiscounts_When_Calculating_Then_TotalIsSumOfLines()
    {
        var request = new PremiumCalculationRequest
        {
            ClaimFreeYears = 4,
            ActivePolicyCount = 2,
            FullYearsAsCustomer = 10
        };

        var breakdown = _calculator.Calculate(request);

        Assert.Equal(3, breakdown.Lines.Count);
        Assert.Equal(0.34, breakdown.Total, 5);
    }

    [Fact]
    public void Given_NoHistory_When_Calculating_Then_NoDiscount()
    {
        var breakdown = _calculator.Calculate(new PremiumCalculationRequest());

        Assert.Equal(0, breakdown.Total);
    }
}
