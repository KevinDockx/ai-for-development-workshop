using ClaimHub.Domain.Entities;
using ClaimHub.Domain.Enums;
using ClaimHub.Domain.Models;
using ClaimHub.Domain.Rules;

namespace ClaimHub.Domain.Services;

/// <summary>
/// Combines eligibility, fraud, financial and SLA checks of one claim into a single decision summary.
/// </summary>
public class ClaimDecisionService
{
    private readonly ClaimEligibilityService _eligibility;
    private readonly FraudDetectionService _fraud;
    private readonly ClaimReserveService _reserves;
    private readonly ClaimSlaService _sla;

    public ClaimDecisionService()
        : this(new ClaimEligibilityService(), new FraudDetectionService(), new ClaimReserveService(), new ClaimSlaService())
    {
    }

    public ClaimDecisionService(
        ClaimEligibilityService eligibility,
        FraudDetectionService fraud,
        ClaimReserveService reserves,
        ClaimSlaService sla)
    {
        _eligibility = eligibility;
        _fraud = fraud;
        _reserves = reserves;
        _sla = sla;
    }

    public ClaimDecisionSummary Summarize(Claim claim, Policy policy, IEnumerable<Claim> holderClaims, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(claim);
        ArgumentNullException.ThrowIfNull(policy);
        ArgumentNullException.ThrowIfNull(holderClaims);

        var coverage = claim.Coverage ?? policy.Coverages.FirstOrDefault(c => c.Id == claim.CoverageId);
        var eligibility = _eligibility.Evaluate(policy, claim.DateOfLoss, claim.ReportedAt, coverage?.Type);
        var fraud = _fraud.Assess(claim, policy, coverage, holderClaims);
        var financial = _reserves.Summarize(claim, policy);
        var sla = _sla.Assess(claim, now);

        return new ClaimDecisionSummary
        {
            ClaimId = claim.Id,
            ClaimNumber = claim.ClaimNumber,
            Status = claim.Status,
            Eligibility = eligibility,
            Fraud = fraud,
            Financial = financial,
            Sla = sla,
            Attention = BuildAttention(claim, eligibility, fraud, financial, sla)
        };
    }

    public ClaimQueueSummary SummarizeQueue(IEnumerable<Claim> claims, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(claims);

        var live = claims.Where(c => !c.IsDeleted).ToList();
        var open = live.Where(c => !c.IsFinal && c.Status != ClaimStatus.Rejected).ToList();

        return new ClaimQueueSummary
        {
            OpenCount = open.Count,
            EscalationCount = _sla.FindEscalations(live, now).Count(),
            FlaggedCount = live.Count(c => c.FraudFlags.Count > 0),
            LateReportedCount = live.Count(c => c.IsLateReported),
            TotalPaid = live.Sum(c => c.PaidAmount),
            TotalReserve = open
                .Where(c => c.Policy is not null)
                .Sum(c => _reserves.Summarize(c, c.Policy!).Reserve),
            ByStatus = live
                .GroupBy(c => c.Status)
                .OrderBy(g => g.Key)
                .ToDictionary(g => g.Key, g => g.Count())
        };
    }

    private static IReadOnlyList<ClaimAttentionItem> BuildAttention(
        Claim claim,
        ClaimEligibilityResult eligibility,
        FraudAssessment fraud,
        ClaimFinancialSummary financial,
        SlaAssessment sla)
    {
        var items = new List<ClaimAttentionItem>();

        foreach (string reason in eligibility.Reasons)
        {
            items.Add(new ClaimAttentionItem(ClaimAttentionLevel.Action, reason));
        }

        if (eligibility.IsLateReported)
        {
            items.Add(new ClaimAttentionItem(
                ClaimAttentionLevel.Watch,
                $"Reported {eligibility.DaysBetweenLossAndReport} days after the loss."));
        }

        if (fraud.HasFlags)
        {
            var level = fraud.RiskLevel >= FraudRiskLevel.Medium ? ClaimAttentionLevel.Action : ClaimAttentionLevel.Watch;
            items.Add(new ClaimAttentionItem(level, $"{fraud.RiskLevel} fraud risk (score {fraud.Score}, {fraud.Flags.Count} flag(s))."));
        }

        if (sla.RequiresEscalation)
        {
            items.Add(new ClaimAttentionItem(
                ClaimAttentionLevel.Action,
                $"First assessment is {sla.ElapsedDays - sla.AllowedDays} day(s) overdue (deadline {sla.Deadline:yyyy-MM-dd})."));
        }
        else if (sla.IsBreached && !sla.IsAssessed)
        {
            items.Add(new ClaimAttentionItem(ClaimAttentionLevel.Watch, "The SLA is breached but the claim was already escalated or moved on."));
        }

        if (financial.LimitReached)
        {
            items.Add(new ClaimAttentionItem(
                ClaimAttentionLevel.Watch,
                $"The coverage limit caps the payout at {financial.ExpectedPayout:0.00}."));
        }

        if (claim.AssessedAmount is null && claim.Status == ClaimStatus.UnderAssessment)
        {
            items.Add(new ClaimAttentionItem(ClaimAttentionLevel.Watch, "Assessment has started but no amount is recorded."));
        }

        return items;
    }
}