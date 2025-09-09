using ClaimHub.Domain.Models;

namespace ClaimHub.Domain.Services;

public class PolicyTermCalculator
{
    public PolicyTerm Calculate(DateOnly start) =>
        new(start, start.AddYears(1).AddDays(-1));

    public PolicyTerm Renew(PolicyTerm previous) => Calculate(previous.NextTermStart);

    public bool StartsOnLeapDay(DateOnly start) => start.Month == 2 && start.Day == 29;

    public bool IsWithinTerm(PolicyTerm term, DateOnly date) => term.Contains(date);

    public int DaysRemaining(PolicyTerm term, DateOnly on)
    {
        if (on >= term.End)
        {
            return 0;
        }

        return term.End.DayNumber - (on < term.Start ? term.Start : on).DayNumber;
    }
}
