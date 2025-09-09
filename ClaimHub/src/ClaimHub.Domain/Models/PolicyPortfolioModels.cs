using ClaimHub.Domain.Enums;

namespace ClaimHub.Domain.Models;

public sealed record CoverageTermSummary(
    int CoverageId,
    CoverageType Type,
    decimal Limit,
    decimal Deductible,
    decimal Premium,
    decimal PaidInTerm,
    decimal Remaining,
    decimal UsedPercentage,
    bool HasValidTerms,
    bool IsMandatory)
{
    public bool IsExhausted => Limit > 0m && Remaining <= 0m;

    public bool IsNearLimit => Limit > 0m && UsedPercentage >= CoverageTermSummary.NearLimitPercentage;

    public const decimal NearLimitPercentage = 80m;
}

public sealed class PolicyCoverageSummary
{
    public PolicyCoverageSummary(
        int policyId,
        string policyNumber,
        ProductType productType,
        PolicyStatus status,
        PolicyTerm term,
        IReadOnlyList<CoverageTermSummary> coverages)
    {
        PolicyId = policyId;
        PolicyNumber = policyNumber;
        ProductType = productType;
        Status = status;
        Term = term;
        Coverages = coverages;
    }

    public int PolicyId { get; }

    public string PolicyNumber { get; }

    public ProductType ProductType { get; }

    public PolicyStatus Status { get; }

    public PolicyTerm Term { get; }

    public IReadOnlyList<CoverageTermSummary> Coverages { get; }

    public decimal TotalLimit => Coverages.Sum(c => c.Limit);

    public decimal TotalCoveragePremium => Coverages.Sum(c => c.Premium);

    public decimal TotalPaidInTerm => Coverages.Sum(c => c.PaidInTerm);

    public bool HasInvalidTerms => Coverages.Any(c => !c.HasValidTerms);

    public CoverageTermSummary? MostUsed => Coverages
        .OrderByDescending(c => c.UsedPercentage)
        .ThenBy(c => c.CoverageId)
        .FirstOrDefault();
}

public sealed record PortfolioBucket(string Key, int PolicyCount, decimal AnnualPremium)
{
    public decimal AveragePremium => PolicyCount == 0 ? 0m : Math.Round(AnnualPremium / PolicyCount, 2, MidpointRounding.AwayFromZero);
}

public sealed class PolicyPortfolioSummary
{
    public DateOnly AsOf { get; init; }

    public int PolicyCount { get; init; }

    public int InForceCount { get; init; }

    public decimal TotalAnnualPremium { get; init; }

    public int ExpiringSoonCount { get; init; }

    public int ExpiringWindowDays { get; init; }

    public IReadOnlyList<PortfolioBucket> ByProduct { get; init; } = Array.Empty<PortfolioBucket>();

    public IReadOnlyList<PortfolioBucket> ByStatus { get; init; } = Array.Empty<PortfolioBucket>();

    public IReadOnlyList<PortfolioBucket> ByPaymentFrequency { get; init; } = Array.Empty<PortfolioBucket>();

    public IReadOnlyList<int> ExpiringSoonPolicyIds { get; init; } = Array.Empty<int>();

    public decimal InForceShare => PolicyCount == 0 ? 0m : Math.Round(InForceCount * 100m / PolicyCount, 1, MidpointRounding.AwayFromZero);
}

public sealed record RenewalWindow(DateOnly PolicyEnd, DateOnly OpensOn, DateOnly ClosesOn, int DaysUntilEnd, bool IsOpen);