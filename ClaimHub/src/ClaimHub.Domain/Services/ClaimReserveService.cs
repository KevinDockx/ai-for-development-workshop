using ClaimHub.Domain.Common;
using ClaimHub.Domain.Entities;
using ClaimHub.Domain.Enums;
using ClaimHub.Domain.Models;

namespace ClaimHub.Domain.Services;

public class ClaimReserveService
{
    private readonly CoverageLimitService _limits;
    private readonly ClaimPayoutCalculator _payouts;

    public ClaimReserveService()
        : this(new CoverageLimitService(), new ClaimPayoutCalculator())
    {
    }

    public ClaimReserveService(CoverageLimitService limits, ClaimPayoutCalculator payouts)
    {
        _limits = limits;
        _payouts = payouts;
    }

    public ClaimFinancialSummary Summarize(Claim claim, Policy policy)
    {
        ArgumentNullException.ThrowIfNull(claim);
        ArgumentNullException.ThrowIfNull(policy);

        var coverage = claim.Coverage ?? policy.Coverages.FirstOrDefault(c => c.Id == claim.CoverageId);
        if (coverage is null)
        {
            return new ClaimFinancialSummary
            {
                ClaimId = claim.Id,
                Status = claim.Status,
                AssessedAmount = claim.AssessedAmount,
                PaidAmount = claim.PaidAmount
            };
        }

        var term = new PolicyTerm(policy.StartDate, policy.EndDate);
        decimal paidByOthers = _limits.PaidInTerm(coverage, policy.Claims, term, claim.Id);
        decimal? basis = claim.ApprovedAmount ?? claim.AssessedAmount;

        var payout = basis is { } amount
            ? _payouts.Calculate(amount, coverage.Deductible, coverage.Limit, paidByOthers)
            : null;

        decimal expected = payout?.PayoutAmount ?? 0m;

        return new ClaimFinancialSummary
        {
            ClaimId = claim.Id,
            Status = claim.Status,
            AssessedAmount = claim.AssessedAmount,
            Deductible = coverage.Deductible,
            CoverageLimit = coverage.Limit,
            PaidInTermByOthers = paidByOthers,
            RemainingLimit = MoneyMath.ClampToZero(coverage.Limit - paidByOthers),
            ExpectedPayout = expected,
            PaidAmount = claim.PaidAmount,
            Reserve = ReserveFor(claim, expected),
            LimitReached = payout?.LimitReached ?? false
        };
    }

    public decimal TotalReserve(Policy policy)
    {
        ArgumentNullException.ThrowIfNull(policy);
        return policy.Claims
            .Where(c => !c.IsDeleted)
            .Sum(c => Summarize(c, policy).Reserve);
    }

    private static decimal ReserveFor(Claim claim, decimal expectedPayout)
    {
        if (claim.Status is ClaimStatus.Rejected or ClaimStatus.Withdrawn or ClaimStatus.Closed or ClaimStatus.PaidOut)
        {
            return 0m;
        }

        return MoneyMath.ClampToZero(expectedPayout - claim.PaidAmount);
    }
}
