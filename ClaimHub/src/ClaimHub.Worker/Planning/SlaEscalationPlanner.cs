using ClaimHub.Domain.Common;
using ClaimHub.Domain.Entities;
using ClaimHub.Worker.Scheduling;

namespace ClaimHub.Worker.Planning;

public sealed record SlaEscalation(Claim Claim, ApplicationUser Handler, int DaysOpen);

/// <summary>Picks the claims that waited too long and spreads them over the senior handlers by current workload.</summary>
public sealed class SlaEscalationPlanner
{
    public int CalendarDaysOpen(Claim claim, DateOnly today) =>
        today.DayNumber - BelgianTime.ToLocalDate(claim.ReportedAt).DayNumber;

    public IReadOnlyList<Claim> SelectOverdue(IEnumerable<Claim> openClaims, DateOnly today, int escalationAfterDays) =>
        openClaims.Where(c => CalendarDaysOpen(c, today) > escalationAfterDays).ToList();

    public ApplicationUser PickLeastLoaded(IReadOnlyList<ApplicationUser> seniors, IReadOnlyDictionary<int, int> workload) =>
        seniors.OrderBy(s => workload[s.Id]).ThenBy(s => s.Id).First();

    public IReadOnlyList<SlaEscalation> Assign(
        IReadOnlyList<Claim> overdue,
        IReadOnlyList<ApplicationUser> seniors,
        Dictionary<int, int> workload,
        DateOnly today)
    {
        var plan = new List<SlaEscalation>();
        foreach (var claim in overdue)
        {
            var senior = PickLeastLoaded(seniors, workload);
            workload[senior.Id]++;
            plan.Add(new SlaEscalation(claim, senior, CalendarDaysOpen(claim, today)));
        }

        return plan;
    }

    public JobRunResult NothingOverdue(int openCount) =>
        new(openCount, 0, openCount, "no claim has waited long enough");

    public JobRunResult NoSeniorAvailable(int openCount) =>
        new(openCount, 0, openCount, "no senior claims handler available");

    public JobRunResult Escalated(int openCount, int escalated, int seniorCount, int afterDays) =>
        new(openCount, escalated, openCount - escalated, $"escalated to {seniorCount} senior handler(s) after {afterDays} calendar days");
}