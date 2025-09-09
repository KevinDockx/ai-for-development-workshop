using ClaimHub.Domain.Enums;

namespace ClaimHub.Domain.Models;

public enum CustomerTier
{
    New = 0,
    Established = 1,
    Loyal = 2,
    Vip = 3
}

public sealed record HolderPolicyLine(
    int PolicyId,
    string PolicyNumber,
    ProductType ProductType,
    PolicyStatus Status,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal AnnualPremium,
    int ClaimCount,
    decimal PaidClaims,
    decimal OutstandingInvoices)
{
    public decimal LossRatio => AnnualPremium <= 0m ? 0m : Math.Round(PaidClaims / AnnualPremium, 2, MidpointRounding.AwayFromZero);
}

public sealed class PolicyHolderOverview
{
    public int PolicyHolderId { get; init; }

    public string FullName { get; init; } = string.Empty;

    public DateOnly AsOf { get; init; }

    public int YearsAsCustomer { get; init; }

    public CustomerTier Tier { get; init; }

    public IReadOnlyList<HolderPolicyLine> Policies { get; init; } = Array.Empty<HolderPolicyLine>();

    public int InForceCount { get; init; }

    public int OpenClaimCount { get; init; }

    public decimal CreditLimit { get; init; }

    public DateOnly? NextExpiry { get; init; }

    public IReadOnlyList<string> Notes { get; init; } = Array.Empty<string>();

    public decimal TotalAnnualPremium => Policies.Where(p => p.Status == PolicyStatus.Active).Sum(p => p.AnnualPremium);

    public decimal TotalPaidClaims => Policies.Sum(p => p.PaidClaims);

    public decimal TotalOutstanding => Policies.Sum(p => p.OutstandingInvoices);

    public decimal CreditHeadroom => CreditLimit - TotalOutstanding;

    public bool IsOverCreditLimit => CreditLimit > 0m && TotalOutstanding > CreditLimit;
}