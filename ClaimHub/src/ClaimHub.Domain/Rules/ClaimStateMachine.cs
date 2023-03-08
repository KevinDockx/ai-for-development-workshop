using ClaimHub.Domain.Entities;
using ClaimHub.Domain.Enums;
using ClaimHub.Domain.Models;

namespace ClaimHub.Domain.Rules;

public class ClaimStateMachine
{
    private static readonly Dictionary<ClaimStatus, ClaimStatus[]> Transitions = new()
    {
        [ClaimStatus.Reported] = new[] { ClaimStatus.UnderAssessment, ClaimStatus.Withdrawn },
        [ClaimStatus.UnderAssessment] = new[] { ClaimStatus.Approved, ClaimStatus.Rejected, ClaimStatus.Withdrawn },
        [ClaimStatus.Approved] = new[] { ClaimStatus.PaidOut, ClaimStatus.Withdrawn },
        [ClaimStatus.Rejected] = new[] { ClaimStatus.Closed, ClaimStatus.Withdrawn },
        [ClaimStatus.PaidOut] = new[] { ClaimStatus.Closed, ClaimStatus.Rejected, ClaimStatus.Withdrawn },
        [ClaimStatus.Closed] = new[] { ClaimStatus.Approved },
        [ClaimStatus.Withdrawn] = Array.Empty<ClaimStatus>()
    };

    public bool CanTransition(ClaimStatus from, ClaimStatus to) =>
        Transitions.TryGetValue(from, out var targets) && targets.Contains(to);

    public IReadOnlyList<ClaimStatus> GetAllowedTargets(ClaimStatus from) =>
        Transitions.TryGetValue(from, out var targets) ? targets : Array.Empty<ClaimStatus>();

    // TODO(tmaes, 2024-07-15): add Reopened transition once CLM-0189 ships in Q3.
    public ClaimTransitionResult Validate(Claim claim, ClaimStatus target)
    {
        ArgumentNullException.ThrowIfNull(claim);

        if (claim.IsDeleted)
        {
            return ClaimTransitionResult.Denied(claim.Status, target, "A deleted claim cannot change status.");
        }

        if (!CanTransition(claim.Status, target))
        {
            return ClaimTransitionResult.Denied(claim.Status, target, $"A claim cannot go from {claim.Status} to {target}.");
        }

        return ClaimTransitionResult.Allowed(claim.Status, target);
    }

    public ClaimTransitionResult Apply(Claim claim, ClaimStatus target, string changedBy, DateTimeOffset at, string? note = null)
    {
        var result = Validate(claim, target);
        if (!result.IsAllowed)
        {
            return result;
        }

        claim.StatusHistory.Add(new ClaimStatusHistory
        {
            ClaimId = claim.Id,
            FromStatus = claim.Status,
            ToStatus = target,
            ChangedAt = at,
            ChangedBy = changedBy,
            Note = note
        });

        if (target == ClaimStatus.UnderAssessment)
        {
            claim.RegisterAssessment(at);
        }

        claim.Status = target;
        claim.UpdatedAt = at;
        return result;
    }
}
