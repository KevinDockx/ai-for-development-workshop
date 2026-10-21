using ClaimHub.Domain.Entities;
using ClaimHub.Domain.Enums;
using ClaimHub.Domain.Models;

namespace ClaimHub.Domain.Services;

public class PolicyPortfolioService
{
    public const int DefaultExpiringWindowDays = 30;
    public const int RenewalLeadDays = 60;

    private readonly CoverageLimitService _limits;

    public PolicyPortfolioService()
        : this(new CoverageLimitService())
    {
    }

    public PolicyPortfolioService(CoverageLimitService limits)
    {
        _limits = limits;
    }

    public PolicyCoverageSummary SummarizeCoverage(Policy policy)
    {
        ArgumentNullException.ThrowIfNull(policy);

        var term = new PolicyTerm(policy.StartDate, policy.EndDate);
        var claims = policy.Claims.Where(c => !c.IsDeleted).ToList();

        var coverages = policy.Coverages
            .Where(c => c.IsActive)
            .OrderBy(c => c.Type)
            .Select(c => ToTermSummary(policy, c, claims, term))
            .ToList();

        return new PolicyCoverageSummary(
            policy.Id, policy.PolicyNumber, policy.ProductType, policy.Status, term, coverages);
    }

    public PolicyPortfolioSummary Summarize(IEnumerable<Policy> policies, DateOnly asOf, int expiringWindowDays = DefaultExpiringWindowDays)
    {
        ArgumentNullException.ThrowIfNull(policies);
        if (expiringWindowDays < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(expiringWindowDays), "The expiring window cannot be negative.");
        }

        var all = policies.Where(p => !p.IsDeleted).ToList();
        var expiring = all
            .Where(p => IsExpiringWithin(p, asOf, expiringWindowDays))
            .OrderBy(p => p.EndDate)
            .ThenBy(p => p.Id)
            .ToList();

        return new PolicyPortfolioSummary
        {
            AsOf = asOf,
            PolicyCount = all.Count,
            InForceCount = all.Count(p => p.IsInForceOn(asOf)),
            TotalAnnualPremium = all.Where(p => p.IsInForceOn(asOf)).Sum(p => p.AnnualPremium),
            ExpiringSoonCount = expiring.Count,
            ExpiringWindowDays = expiringWindowDays,
            ExpiringSoonPolicyIds = expiring.Select(p => p.Id).ToList(),
            ByProduct = Bucket(all, p => p.ProductType.ToString()),
            ByStatus = Bucket(all, p => p.Status.ToString()),
            ByPaymentFrequency = Bucket(all.Where(p => p.IsInForceOn(asOf)), p => p.PaymentFrequency.ToString())
        };
    }

    public bool IsExpiringWithin(Policy policy, DateOnly asOf, int windowDays)
    {
        if (!policy.IsInForceOn(asOf))
        {
            return false;
        }

        int daysLeft = policy.EndDate.DayNumber - asOf.DayNumber;
        return daysLeft <= windowDays;
    }

    public RenewalWindow GetRenewalWindow(Policy policy, DateOnly asOf)
    {
        ArgumentNullException.ThrowIfNull(policy);

        var opensOn = policy.EndDate.AddDays(-RenewalLeadDays);
        int daysUntilEnd = policy.EndDate.DayNumber - asOf.DayNumber;
        bool isOpen = policy.Status == PolicyStatus.Active && asOf >= opensOn && asOf < policy.EndDate;

        return new RenewalWindow(policy.EndDate, opensOn, policy.EndDate.AddDays(-1), daysUntilEnd, isOpen);
    }

    public IReadOnlyList<CoverageTermSummary> NearLimit(IEnumerable<Policy> policies) =>
        policies
            .Where(p => !p.IsDeleted && p.Status == PolicyStatus.Active)
            .SelectMany(p => SummarizeCoverage(p).Coverages)
            .Where(c => c.IsNearLimit)
            .OrderByDescending(c => c.UsedPercentage)
            .ThenBy(c => c.CoverageId)
            .ToList();

    private CoverageTermSummary ToTermSummary(Policy policy, Coverage coverage, IReadOnlyCollection<Claim> claims, PolicyTerm term)
    {
        var usage = _limits.GetUsage(coverage, claims, term);

        return new CoverageTermSummary(
            coverage.Id,
            coverage.Type,
            coverage.Limit,
            coverage.Deductible,
            coverage.Premium,
            usage.PaidInTerm,
            usage.Remaining,
            usage.UsedPercentage,
            _limits.HasValidTerms(coverage),
            coverage.IsMandatoryFor(policy.ProductType));
    }

    private static IReadOnlyList<PortfolioBucket> Bucket(IEnumerable<Policy> policies, Func<Policy, string> keySelector) =>
        policies
            .GroupBy(keySelector)
            .Select(g => new PortfolioBucket(g.Key, g.Count(), g.Sum(p => p.AnnualPremium)))
            .OrderByDescending(b => b.AnnualPremium)
            .ThenBy(b => b.Key, StringComparer.Ordinal)
            .ToList();
}