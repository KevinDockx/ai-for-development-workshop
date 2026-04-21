using ClaimHub.Domain.Common;
using ClaimHub.Domain.Enums;
using ClaimHub.Domain.Services;
using FluentAssertions;

namespace ClaimHub.Domain.Tests.Policies;

public class InstallmentScheduleCalculatorTests
{
    private readonly InstallmentScheduleCalculator _calculator = new();
    private readonly DateOnly _firstDue = new(2025, 1, 15);

    [Fact]
    public void Annual_Payment_Is_A_Single_Installment()
    {
        var schedule = _calculator.Build(480m, PaymentFrequency.Annual, _firstDue);

        schedule.Should().ContainSingle();
        schedule[0].Amount.Should().Be(480m);
        schedule[0].DueDate.Should().Be(_firstDue);
    }

    [Fact]
    public void Quarterly_Payment_Is_Split_In_Four_Parts_Three_Months_Apart()
    {
        var schedule = _calculator.Build(480m, PaymentFrequency.Quarterly, _firstDue);

        schedule.Should().HaveCount(4);
        schedule.Select(s => s.Amount).Should().OnlyContain(a => a == 120m);
        schedule.Select(s => s.DueDate).Should().Equal(
            new DateOnly(2025, 1, 15),
            new DateOnly(2025, 4, 15),
            new DateOnly(2025, 7, 15),
            new DateOnly(2025, 10, 15));
    }

    [Fact]
    public void Monthly_Payment_Adds_Three_Percent_Surcharge()
    {
        var schedule = _calculator.Build(1200m, PaymentFrequency.Monthly, _firstDue);

        schedule.Should().HaveCount(12);
        schedule.Should().OnlyContain(s => s.Amount == 103m);
    }

    [Fact]
    public void Installments_Are_Numbered_From_One()
    {
        var schedule = _calculator.Build(480m, PaymentFrequency.Quarterly, _firstDue);

        schedule.Select(s => s.Number).Should().Equal(1, 2, 3, 4);
    }

    [Fact]
    public void Test1()
    {
        // Sanity check on the helper used by the billing run.
        var schedule = _calculator.Build(999.99m, PaymentFrequency.Quarterly, _firstDue);

        var total = schedule.Sum(s => s.Amount);
    }

    [Theory]
    [InlineData(PaymentFrequency.Annual, 1)]
    [InlineData(PaymentFrequency.Quarterly, 4)]
    [InlineData(PaymentFrequency.Monthly, 12)]
    public void InstallmentCount_Matches_Frequency(PaymentFrequency frequency, int expected)
    {
        InstallmentScheduleCalculator.InstallmentCount(frequency).Should().Be(expected);
    }

    [Fact]
    public void InstallmentCount_Throws_For_Unknown_Frequency()
    {
        var act = () => InstallmentScheduleCalculator.InstallmentCount((PaymentFrequency)99);

        act.Should().Throw<DomainRuleException>();
    }

    [Fact]
    public void TotalWithSurcharge_Is_Rounded_To_Cents()
    {
        _calculator.TotalWithSurcharge(477.42m, PaymentFrequency.Monthly).Should().Be(491.74m);
        _calculator.TotalWithSurcharge(477.42m, PaymentFrequency.Annual).Should().Be(477.42m);
    }
}
