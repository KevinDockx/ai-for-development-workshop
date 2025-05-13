using ClaimHub.Domain.Entities;
using ClaimHub.Domain.Enums;
using ClaimHub.Domain.Models;

namespace ClaimHub.Domain.Services;

public class RenewalOutlookService
{
    public const decimal ReviewLossRatioPercent = 60m;
    public const decimal UnderwritingLossRatioPercent = 100m;
    public const int ClaimCountForReview = 2;

    private readonly PolicyPortfolioService _portfolio;

    public RenewalOutlookService()
        : this(new PolicyPortfolioService())
    {
    }

    public RenewalOutlookService(PolicyPortfolioService portfolio)
    {
        _portfolio = portfolio;
    }

    public RenewalOutlook Evaluate(Policy policy, DateOnly asOf)
    {
        ArgumentNullException.ThrowIfNull(policy);

        var counted = policy.Claims
            .Where(c => !c.IsDeleted && c.Status is not (ClaimStatus.Rejected or ClaimStatus.Withdrawn))
            .ToList();

        decimal cost = counted.Sum(c => c.PaidAmount > 0m ? c.PaidAmount : c.ApprovedAmount ?? 0m);
        decimal lossRatio = policy.AnnualPremium <= 0m
            ? 0m
            : Math.Round(cost * 100m / policy.AnnualPremium, 1, MidpointRounding.AwayFromZero);

        var findings = new List<RenewalFinding>();
        AddFindings(policy, counted, lossRatio, findings);

        return new RenewalOutlook
        {
            PolicyId = policy.Id,
            Advice = Decide(lossRatio, counted.Count, policy),
            ClaimCount = counted.Count,
            OpenClaimCount = counted.Count(c => !c.IsFinal),
            ClaimCost = cost,
            AnnualPremium = policy.AnnualPremium,
            LossRatioPercent = lossRatio,
            Window = _portfolio.GetRenewalWindow(policy, asOf),
            Findings = findings
        };
    }

    private static RenewalAdvice Decide(decimal lossRatio, int claimCount, Policy policy)
    {
        if (lossRatio >= UnderwritingLossRatioPercent || policy.Status == PolicyStatus.Suspended)
        {
            return RenewalAdvice.ManualUnderwriting;
        }

        return lossRatio >= ReviewLossRatioPercent || claimCount >= ClaimCountForReview
            ? RenewalAdvice.ReviewPremium
            : RenewalAdvice.RenewAsIs;
    }

    private static void AddFindings(Policy policy, IReadOnlyCollection<Claim> claims, decimal lossRatio, List<RenewalFinding> findings)
    {
        if (policy.Status == PolicyStatus.Suspended)
        {
            findings.Add(new RenewalFinding("SUSPENDED", "The policy is suspended and cannot renew automatically."));
        }

        if (lossRatio >= UnderwritingLossRatioPercent)
        {
            findings.Add(new RenewalFinding("LOSS_RATIO", $"Claims cost {lossRatio:0.#}% of the annual premium."));
        }
        else if (lossRatio >= ReviewLossRatioPercent)
        {
            findings.Add(new RenewalFinding("LOSS_RATIO_HIGH", $"Claims already cost {lossRatio:0.#}% of the annual premium."));
        }

        if (claims.Count >= ClaimCountForReview)
        {
            findings.Add(new RenewalFinding("CLAIM_FREQUENCY", $"{claims.Count} claims were filed during the term."));
        }

        int open = claims.Count(c => !c.IsFinal);
        if (open > 0)
        {
            findings.Add(new RenewalFinding("OPEN_CLAIMS", $"{open} claim(s) are still open and may change the loss ratio."));
        }

        if (claims.Any(c => c.IsLateReported))
        {
            findings.Add(new RenewalFinding("LATE_REPORTED", "At least one claim was reported late."));
        }
    }
}