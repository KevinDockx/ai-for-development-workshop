using ClaimHub.Domain.Common;
using ClaimHub.Domain.Entities;
using ClaimHub.Domain.Enums;
using ClaimHub.Domain.Models;

namespace ClaimHub.Domain.Rules;

public class ClaimSlaService
{
    public const int FirstAssessmentDays = 5;

    // TODO(sjanssens, 2024-01-19): business days instead of calendar days (CLM-0158).
    public SlaAssessment Assess(Claim claim, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(claim);

        var reportedOn = BelgianTime.ToLocalDate(claim.ReportedAt);
        var today = BelgianTime.ToLocalDate(now);
        var deadline = reportedOn.AddDays(FirstAssessmentDays);
        bool assessed = claim.FirstAssessmentAt.HasValue;

        int elapsed = (assessed ? BelgianTime.ToLocalDate(claim.FirstAssessmentAt!.Value) : today).DayNumber - reportedOn.DayNumber;
        bool breached = elapsed > FirstAssessmentDays;

        return new SlaAssessment
        {
            ClaimId = claim.Id,
            ReportedOn = reportedOn,
            Deadline = deadline,
            ElapsedDays = elapsed,
            AllowedDays = FirstAssessmentDays,
            IsAssessed = assessed,
            IsBreached = breached,
            RequiresEscalation = !assessed && breached && claim.EscalatedAt is null && claim.Status == ClaimStatus.Reported
        };
    }

    public IEnumerable<Claim> FindEscalations(IEnumerable<Claim> claims, DateTimeOffset now) =>
        claims.Where(c => !c.IsDeleted && Assess(c, now).RequiresEscalation);

    public void Escalate(Claim claim, ApplicationUser seniorHandler, DateTimeOffset at)
    {
        if (seniorHandler.Role != UserRole.SeniorClaimsHandler)
        {
            throw new DomainRuleException("Escalations go to a senior claims handler.");
        }

        claim.AssignedHandlerId = seniorHandler.Id;
        claim.EscalatedAt = at;
        claim.UpdatedAt = at;
    }
}
