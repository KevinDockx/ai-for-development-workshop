namespace ClaimHub.Domain.Models;

public enum RenewalAdvice
{
    RenewAsIs = 0,
    ReviewPremium = 1,
    ManualUnderwriting = 2
}

public sealed record RenewalFinding(string Code, string Message);

/// <summary>Advisory view on how a policy has performed over its current term, shown before a renewal is triggered.</summary>
public sealed class RenewalOutlook
{
    public int PolicyId { get; init; }

    public RenewalAdvice Advice { get; init; }

    public int ClaimCount { get; init; }

    public int OpenClaimCount { get; init; }

    public decimal ClaimCost { get; init; }

    public decimal AnnualPremium { get; init; }

    /// <summary>Claim cost as a percentage of the annual premium, rounded to one decimal.</summary>
    public decimal LossRatioPercent { get; init; }

    public RenewalWindow Window { get; init; } = null!;

    public IReadOnlyList<RenewalFinding> Findings { get; init; } = Array.Empty<RenewalFinding>();

    public bool HasFindings => Findings.Count > 0;
}