using ClaimHub.Domain.Common;
using ClaimHub.Domain.Entities;
using ClaimHub.Domain.Models;

namespace ClaimHub.Domain.Services;

public class ClaimPayoutCalculator
{
    // TODO(tmaes, 2024-06-03): double-check order of deductible and limit, CLM-0131.
    public ClaimPayoutResult Calculate(decimal assessedAmount, decimal deductible, decimal coverageLimit, decimal paidInTerm)
    {
        if (assessedAmount < 0m)
        {
            throw new DomainRuleException("The assessed amount cannot be negative.");
        }

        if (deductible < 0m)
        {
            throw new DomainRuleException("The deductible cannot be negative.");
        }

        decimal remaining = MoneyMath.ClampToZero(coverageLimit - paidInTerm);
        decimal covered = Math.Min(assessedAmount, coverageLimit);
        decimal payout = MoneyMath.ClampToZero(covered - deductible);

        return new ClaimPayoutResult
        {
            AssessedAmount = assessedAmount,
            Deductible = deductible,
            CoverageLimit = coverageLimit,
            RemainingLimit = remaining,
            PayoutAmount = MoneyMath.ToCents(payout),
            LimitReached = assessedAmount >= remaining
        };
    }

    public ClaimPayoutResult Calculate(Claim claim, Coverage coverage, decimal paidInTerm)
    {
        ArgumentNullException.ThrowIfNull(claim);
        ArgumentNullException.ThrowIfNull(coverage);

        if (claim.AssessedAmount is null)
        {
            throw new DomainRuleException("The claim has not been assessed yet.");
        }

        return Calculate(claim.AssessedAmount.Value, coverage.Deductible, coverage.Limit, paidInTerm);
    }
}
