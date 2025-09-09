using ClaimHub.Domain.Common;
using ClaimHub.Domain.Entities;
using ClaimHub.Domain.Enums;
using ClaimHub.Domain.Models;

namespace ClaimHub.Domain.Services;

/// <summary>
/// Shows how open claims are spread over the claims handlers, including claims nobody has picked up yet.
/// </summary>
public class HandlerWorkloadService
{
    public const int DefaultOverloadThreshold = 8;

    public static bool IsOpen(Claim claim) =>
        !claim.IsDeleted && claim.Status is ClaimStatus.Reported or ClaimStatus.UnderAssessment;

    public HandlerWorkloadReport Build(IEnumerable<ApplicationUser> handlers, IEnumerable<Claim> claims, DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(handlers);
        ArgumentNullException.ThrowIfNull(claims);

        var open = claims.Where(IsOpen).ToList();
        var unassigned = open.Where(c => c.AssignedHandlerId is null).ToList();

        var lines = handlers
            .Where(h => h.IsActive && h.CanAssessClaims)
            .OrderBy(h => h.Id)
            .Select(h => ToLine(h, open.Where(c => c.AssignedHandlerId == h.Id).ToList(), today))
            .ToList();

        return new HandlerWorkloadReport
        {
            AsOf = today,
            Handlers = lines,
            UnassignedOpenClaims = unassigned.Count,
            OldestUnassignedDays = unassigned.Count == 0 ? 0 : unassigned.Max(c => DaysOpen(c, today))
        };
    }

    public IReadOnlyList<HandlerWorkloadLine> Overloaded(HandlerWorkloadReport report, int threshold = DefaultOverloadThreshold) =>
        report.Handlers.Where(h => h.IsOverloaded(threshold)).ToList();

    private static HandlerWorkloadLine ToLine(ApplicationUser handler, IReadOnlyList<Claim> assigned, DateOnly today) =>
        new(
            handler.Id,
            handler.DisplayName,
            handler.Role == UserRole.SeniorClaimsHandler,
            assigned.Count,
            assigned.Count(c => c.EscalatedAt != null),
            assigned.Sum(c => c.AssessedAmount ?? 0m),
            assigned.Count == 0 ? 0 : assigned.Max(c => DaysOpen(c, today)));

    private static int DaysOpen(Claim claim, DateOnly today) =>
        today.DayNumber - BelgianTime.ToLocalDate(claim.ReportedAt).DayNumber;
}