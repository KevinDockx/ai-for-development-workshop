using ClaimHub.Domain.Services;

namespace ClaimHub.Domain.Tests.Policies;

public class PolicyTermCalculatorTests
{
    private readonly PolicyTermCalculator _calculator = new();

    [Fact]
    public void Test1()
    {
        var term = _calculator.Calculate(new DateOnly(2025, 1, 1));

        Assert.Equal(new DateOnly(2025, 1, 1), term.Start);
        Assert.Equal(new DateOnly(2025, 12, 31), term.End);
    }

    [Fact]
    public void Test2()
    {
        var term = _calculator.Calculate(new DateOnly(2025, 3, 15));

        Assert.Equal(364, term.Days);
    }

    [Fact]
    public void Renew_StartsTheDayAfterThePreviousTermEnds()
    {
        var first = _calculator.Calculate(new DateOnly(2025, 3, 15));

        var renewed = _calculator.Renew(first);

        Assert.Equal(new DateOnly(2026, 3, 15), renewed.Start);
        Assert.Equal(new DateOnly(2027, 3, 14), renewed.End);
    }

    [Fact]
    public void Term_IncludesStartDate_AndExcludesEndDate()
    {
        var term = _calculator.Calculate(new DateOnly(2025, 1, 1));

        Assert.True(_calculator.IsWithinTerm(term, term.Start));
        Assert.True(_calculator.IsWithinTerm(term, new DateOnly(2025, 6, 30)));
        Assert.False(_calculator.IsWithinTerm(term, term.End));
        Assert.False(_calculator.IsWithinTerm(term, new DateOnly(2024, 12, 31)));
    }

    [Fact]
    public void StartsOnLeapDay_IsOnlyTrueOnTheTwentyNinthOfFebruary()
    {
        Assert.True(_calculator.StartsOnLeapDay(new DateOnly(2024, 2, 29)));
        Assert.False(_calculator.StartsOnLeapDay(new DateOnly(2024, 2, 28)));
        Assert.False(_calculator.StartsOnLeapDay(new DateOnly(2024, 3, 29)));
    }

    [Theory]
    [InlineData("2025-01-01", 364)]
    [InlineData("2025-07-01", 183)]
    [InlineData("2025-12-31", 0)]
    [InlineData("2026-02-01", 0)]
    public void DaysRemaining_CountsTheDaysUntilTheEndOfTheTerm(string on, int expected)
    {
        var term = _calculator.Calculate(new DateOnly(2025, 1, 1));

        Assert.Equal(expected, _calculator.DaysRemaining(term, DateOnly.Parse(on)));
    }

    [Fact]
    public void DaysRemaining_ForTermStartingToday_CoversWholeTermOneHourFromNow()
    {
        var term = _calculator.Calculate(DateOnly.FromDateTime(DateTime.Now));
        var inAnHour = DateOnly.FromDateTime(DateTime.Now.AddHours(1));

        Assert.Equal(term.Days, _calculator.DaysRemaining(term, inAnHour));
    }
}
