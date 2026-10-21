using ClaimHub.Domain.Entities;
using ClaimHub.Domain.Enums;
using ClaimHub.Domain.Models;

namespace ClaimHub.Domain.Rules;

public class FraudDetectionService
{
    public FraudAssessment Assess(Claim claim, Policy policy, Coverage? coverage, IEnumerable<Claim> policyHolderClaims)
    {
        ArgumentNullException.ThrowIfNull(claim);
        ArgumentNullException.ThrowIfNull(policy);

        var assessment = new FraudAssessment { ClaimId = claim.Id };

        var windowStart = claim.ReportedAt.AddMonths(-12);
        int recentClaims = policyHolderClaims.Count(c =>
            !c.IsDeleted && c.Id != claim.Id && c.ReportedAt > windowStart && c.ReportedAt <= claim.ReportedAt) + 1;

        if (recentClaims > 2)
        {
            assessment.Flags.Add(Raise(claim, FraudFlagType.FrequentClaims, 25,
                $"{recentClaims} claims reported in the last 12 months."));
        }

        int daysAfterStart = claim.DateOfLoss.DayNumber - policy.StartDate.DayNumber;
        if (daysAfterStart is >= 0 and <= 14)
        {
            assessment.Flags.Add(Raise(claim, FraudFlagType.LossShortlyAfterStart, 30,
                $"Loss occurred {daysAfterStart} days after the policy start."));
        }

        if (coverage is not null && claim.AssessedAmount is { } assessed && assessed > coverage.Limit * 0.8m)
        {
            assessment.Flags.Add(Raise(claim, FraudFlagType.HighAmountComparedToLimit, 20,
                $"Assessed amount {assessed:N2} exceeds 80% of the {coverage.Limit:N2} limit."));
        }

        return assessment;
    }

    private static FraudFlag Raise(Claim claim, FraudFlagType type, int weight, string details) =>
        new()
        {
            ClaimId = claim.Id,
            Type = type,
            Weight = weight,
            Details = details,
            RaisedAt = DateTimeOffset.UtcNow
        };
}
