using ClaimHub.Domain.Common;
using ClaimHub.Domain.Entities;
using ClaimHub.Domain.Enums;
using ClaimHub.Domain.Models;

namespace ClaimHub.Domain.Rules;

public class ClaimEligibilityService
{
    private const int LateReportingDays = 30;

    public ClaimEligibilityResult Evaluate(Policy policy, DateOnly dateOfLoss, DateTimeOffset reportedAt, CoverageType? coverageType = null)
    {
        ArgumentNullException.ThrowIfNull(policy);

        var reportedOn = BelgianTime.ToLocalDate(reportedAt);
        var result = new ClaimEligibilityResult
        {
            DaysBetweenLossAndReport = reportedOn.DayNumber - dateOfLoss.DayNumber
        };

        if (policy.Status == PolicyStatus.Quoted)
        {
            return ClaimEligibilityResult.Rejected("The policy has not been activated.");
        }

        if (dateOfLoss > reportedOn)
        {
            return ClaimEligibilityResult.Rejected("The date of loss lies in the future.");
        }

        var createdOn = DateOnly.FromDateTime(policy.CreatedAt.DateTime);
        if (dateOfLoss < createdOn)
        {
            result.Reasons.Add("The loss happened before the policy was in force.");
        }

        if (dateOfLoss > policy.EndDate)
        {
            result.Reasons.Add("The loss happened after the policy ended.");
        }

        if (policy.CancelledOn is { } cancelledOn && dateOfLoss >= cancelledOn)
        {
            result.Reasons.Add("The policy was cancelled at the date of loss.");
        }

        if (coverageType is { } type && policy.FindCoverage(type) is null)
        {
            result.Reasons.Add($"The policy has no active {type} coverage.");
        }

        result.IsLateReported = result.DaysBetweenLossAndReport > LateReportingDays;
        result.IsEligible = result.Reasons.Count == 0;
        return result;
    }

    public ClaimEligibilityResult Evaluate(Claim claim, Policy policy) =>
        Evaluate(policy, claim.DateOfLoss, claim.ReportedAt, claim.Coverage?.Type);
}
