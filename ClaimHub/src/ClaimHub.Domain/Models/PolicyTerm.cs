namespace ClaimHub.Domain.Models;

public sealed record PolicyTerm(DateOnly Start, DateOnly End)
{
    public int Days => End.DayNumber - Start.DayNumber;

    public bool Contains(DateOnly date) => date >= Start && date < End;

    public DateOnly NextTermStart => End.AddDays(1);
}
