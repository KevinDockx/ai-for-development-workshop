using ClaimHub.Domain.Enums;
using ClaimHub.Domain.Services;
using ClaimHub.Domain.Tests.Support;
using FluentAssertions;

namespace ClaimHub.Domain.Tests.Policies;

public class CancellationRefundCalculatorTests
{
    private readonly CancellationRefundCalculator _calculator = new();

    [Fact]
    public void Quoted_Policy_Has_Nothing_To_Refund()
    {
        var policy = new PolicyBuilder().Build();

        var result = _calculator.Calculate(policy, new DateOnly(2025, 3, 1));

        result.HasRefund.Should().BeFalse();
        result.RefundAmount.Should().Be(0m);
        result.Message.Should().Contain("quote");
    }

    [Fact]
    public void Active_Policy_Reports_Unused_Days_Until_End_Date()
    {
        var policy = new PolicyBuilder().WithStatus(PolicyStatus.Active).Build();

        var result = _calculator.Calculate(policy, new DateOnly(2025, 7, 1));

        result.UnusedDays.Should().Be(183);
        result.TermDays.Should().Be(364);
        result.AnnualPremium.Should().Be(600m);
    }

    [Fact]
    public void Cancellation_Before_Start_Date_Is_Moved_To_Start_Date()
    {
        var policy = new PolicyBuilder().WithStatus(PolicyStatus.Active).Build();

        var result = _calculator.Calculate(policy, new DateOnly(2024, 11, 1));

        result.EffectiveDate.Should().Be(new DateOnly(2025, 1, 1));
    }

    [Fact]
    public void Cancellation_After_End_Date_Has_No_Unused_Days()
    {
        var policy = new PolicyBuilder().WithStatus(PolicyStatus.Active).Build();

        var result = _calculator.Calculate(policy, new DateOnly(2026, 1, 15));

        result.UnusedDays.Should().Be(0);
        result.HasRefund.Should().BeFalse();
    }

    [Fact]
    public void Mid_Term_Cancellation_Returns_Message()
    {
        var policy = new PolicyBuilder().WithStatus(PolicyStatus.Active).WithAnnualPremium(730m).Build();

        var result = _calculator.Calculate(policy, new DateOnly(2025, 7, 1));

        result.Message.Should().Be("No unused premium to refund.");
    }

    [Fact]
    public void Null_Policy_Throws()
    {
        var act = () => _calculator.Calculate(null!, new DateOnly(2025, 1, 1));

        act.Should().Throw<ArgumentNullException>();
    }
}
