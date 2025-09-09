namespace ClaimHub.Domain.Services;

public class BusinessDayCalculator
{
    public bool IsBusinessDay(DateOnly date) =>
        date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday;

    public DateOnly AddBusinessDays(DateOnly start, int days)
    {
        var current = start;
        int step = days < 0 ? -1 : 1;
        int remaining = Math.Abs(days);

        while (remaining > 0)
        {
            current = current.AddDays(step);
            if (IsBusinessDay(current))
            {
                remaining--;
            }
        }

        return current;
    }

    // Counts the business days after start up to and including end.
    public int CountBusinessDays(DateOnly start, DateOnly end)
    {
        int count = 0;
        for (var day = start.AddDays(1); day <= end; day = day.AddDays(1))
        {
            if (IsBusinessDay(day))
            {
                count++;
            }
        }

        return count;
    }

    public DateOnly NextBusinessDay(DateOnly date) => IsBusinessDay(date) ? date : AddBusinessDays(date, 1);
}
