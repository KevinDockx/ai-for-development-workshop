using ClaimHub.Domain.Entities;
using ClaimHub.Domain.Models;

namespace ClaimHub.Domain.Services;

public class CoverageLimitService
{
    public decimal PaidInTerm(Coverage coverage, IEnumerable<Claim> claims, PolicyTerm term, int? excludeClaimId = null)
    {
        ArgumentNullException.ThrowIfNull(coverage);
        ArgumentNullException.ThrowIfNull(claims);

        return claims
            .Where(c => !c.IsDeleted && c.CoverageId == coverage.Id && c.Id != excludeClaimId)
            .Where(c => term.Contains(c.DateOfLoss))
            .Sum(c => c.PaidAmount);
    }

    public decimal RemainingLimit(Coverage coverage, IEnumerable<Claim> claims, PolicyTerm term, int? excludeClaimId = null)
    {
        decimal remaining = coverage.Limit - PaidInTerm(coverage, claims, term, excludeClaimId);
        return remaining > 0m ? remaining : 0m;
    }

    public CoverageUsage GetUsage(Coverage coverage, IEnumerable<Claim> claims, PolicyTerm term) =>
        new()
        {
            CoverageId = coverage.Id,
            Type = coverage.Type,
            Limit = coverage.Limit,
            PaidInTerm = PaidInTerm(coverage, claims, term)
        };

    public IReadOnlyList<CoverageUsage> GetUsage(Policy policy)
    {
        var term = new PolicyTerm(policy.StartDate, policy.EndDate);
        return policy.Coverages
            .Where(c => c.IsActive)
            .Select(c => GetUsage(c, policy.Claims, term))
            .ToList();
    }

    public bool HasValidTerms(Coverage coverage) =>
        coverage.Limit > 0m && coverage.Deductible >= 0m && coverage.Deductible < coverage.Limit;
}
