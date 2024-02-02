using ClaimHub.Domain.Common;
using ClaimHub.Domain.Enums;
using ClaimHub.Domain.Services;
using ClaimHub.Domain.Tests.Support;
using FluentAssertions;

namespace ClaimHub.Domain.Tests.Policies;

public class PremiumCalculatorTests
{
    private readonly PremiumCalculator _calculator = new();

    [Fact]
    public void Should_Calculate_Annual_Premium_For_Basic_Home_Policy()
    {
        var quote = _calculator.Calculate(TestData.HomeRequest());

        quote.BasePremium.Should().Be(307.50m);
        quote.NetPremium.Should().Be(307.50m);
        quote.TaxAmount.Should().Be(28.44m);
        quote.AnnualPremium.Should().Be(335.94m);
    }

    [Fact]
    public void Should_Use_Default_Tax_Rate_Of_9_25_Percent()
    {
        var quote = _calculator.Calculate(TestData.HomeRequest());

        quote.TaxRate.Should().Be(0.0925m);
    }

    [Fact]
    public void Should_Respect_Custom_Tax_Rate()
    {
        var request = TestData.HomeRequest();
        request.TaxRate = 0.21m;

        var quote = _calculator.Calculate(request);

        quote.TaxAmount.Should().Be(64.58m);
        quote.AnnualPremium.Should().Be(372.08m);
    }

    [Fact]
    public void Should_Add_Contents_Coverage_To_Base_Premium()
    {
        var request = TestData.HomeRequest(
            250_000m, 30_000m, 2, 2000, CoverageType.Building, CoverageType.Contents);

        var quote = _calculator.Calculate(request);

        quote.BasePremium.Should().Be(395.50m);
        quote.CoveragePremiums.Should().ContainKeys(CoverageType.Building, CoverageType.Contents);
    }

    [Theory]
    [InlineData(1, 276.75)]
    [InlineData(2, 307.50)]
    [InlineData(3, 353.63)]
    [InlineData(4, 430.50)]
    public void Should_Apply_Postcode_Zone_Factor(int zone, double expectedNet)
    {
        var quote = _calculator.Calculate(TestData.HomeRequest(zone: zone));

        quote.NetPremium.Should().Be((decimal)expectedNet);
    }

    [Fact]
    public void Should_Rate_Old_Buildings_Higher()
    {
        var modern = _calculator.Calculate(TestData.HomeRequest(constructionYear: 2015));
        var old = _calculator.Calculate(TestData.HomeRequest(constructionYear: 1930));

        old.AnnualPremium.Should().BeGreaterThan(modern.AnnualPremium);
    }

    [Fact]
    public void Should_Calculate_Motor_Premium_For_Experienced_Driver()
    {
        var quote = _calculator.Calculate(TestData.MotorRequest());

        quote.BasePremium.Should().Be(437.00m);
        quote.TaxAmount.Should().Be(40.42m);
        quote.AnnualPremium.Should().Be(477.42m);
    }

    [Fact]
    public void Should_Charge_Young_Drivers_More()
    {
        var quote = _calculator.Calculate(TestData.MotorRequest(driverAge: 19));

        quote.RiskFactor.Should().Be(1.9m);
        quote.NetPremium.Should().Be(830.30m);
        quote.AnnualPremium.Should().Be(907.10m);
    }

    [Fact]
    public void Should_Charge_More_For_Powerful_Cars()
    {
        var weak = _calculator.Calculate(TestData.MotorRequest(powerKw: 70));
        var strong = _calculator.Calculate(TestData.MotorRequest(powerKw: 160));

        strong.RiskFactor.Should().Be(1.35m);
        strong.AnnualPremium.Should().BeGreaterThan(weak.AnnualPremium);
    }

    [Fact]
    public void Should_Add_Comprehensive_Premium_Based_On_Vehicle_Value()
    {
        var request = TestData.MotorRequest();
        request.Coverages.Add(CoverageType.Comprehensive);

        var quote = _calculator.Calculate(request);

        quote.BasePremium.Should().Be(857.00m);
    }

    [Theory]
    [InlineData(TravelPartyType.Single, TravelScope.Europe, 52.00)]
    [InlineData(TravelPartyType.Family, TravelScope.Europe, 98.00)]
    [InlineData(TravelPartyType.Single, TravelScope.Worldwide, 75.40)]
    public void Should_Calculate_Travel_Net_Premium(TravelPartyType party, TravelScope scope, double expectedNet)
    {
        var quote = _calculator.Calculate(TestData.TravelRequest(party, scope));

        quote.NetPremium.Should().Be((decimal)expectedNet);
    }

    [Fact]
    public void Should_Apply_Discounts_Before_Tax()
    {
        var request = TestData.HomeRequest();
        request.ClaimFreeYears = 2;

        var quote = _calculator.Calculate(request);

        quote.DiscountPercent.Should().Be(10.00m);
        quote.NetPremium.Should().Be(276.75m);
        quote.AnnualPremium.Should().Be(302.35m);
    }

    [Fact]
    public void Should_Reject_Motor_Policy_Without_Third_Party_Liability()
    {
        var request = TestData.MotorRequest();
        request.Coverages = new List<CoverageType> { CoverageType.Glass };

        var act = () => _calculator.Calculate(request);

        act.Should().Throw<DomainRuleException>().WithMessage("*mandatory*");
    }

    [Fact]
    public void Should_Reject_Coverage_That_Does_Not_Belong_To_Product()
    {
        var request = TestData.HomeRequest(coverages: CoverageType.Glass);

        var act = () => _calculator.Calculate(request);

        act.Should().Throw<DomainRuleException>();
    }

    [Fact]
    public void Should_Reject_Home_Policy_Without_Coverages()
    {
        var request = TestData.HomeRequest();
        request.Coverages.Clear();

        var act = () => _calculator.Calculate(request);

        act.Should().Throw<DomainRuleException>().WithMessage("*At least one coverage*");
    }

    [Fact]
    public void Should_Throw_For_Null_Request()
    {
        var act = () => _calculator.Calculate(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Should_Calculate_Family_Travel_Quote()
    {
        // Not asserting yet, the figures still have to be agreed with the actuaries.
        var quote = _calculator.Calculate(TestData.TravelRequest(TravelPartyType.Family, TravelScope.Worldwide));

        var installments = quote.Installments;
    }

    [Fact]
    public void Should_Fill_Installment_Schedule_For_Monthly_Payment()
    {
        var request = TestData.MotorRequest();
        request.PaymentFrequency = PaymentFrequency.Monthly;

        var quote = _calculator.Calculate(request);

        quote.Installments.Should().HaveCount(12);
        quote.Installments.Should().OnlyContain(i => i.Amount == 40.98m);
    }

    [Fact]
    public void Should_Round_Amounts_To_Cents_Away_From_Zero()
    {
        MoneyMath.ToCents(2.345m).Should().Be(2.35m);
        MoneyMath.ToCents(-2.345m).Should().Be(-2.35m);
    }
}
