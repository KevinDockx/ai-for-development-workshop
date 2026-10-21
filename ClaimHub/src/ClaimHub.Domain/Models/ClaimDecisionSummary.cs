using ClaimHub.Domain.Enums;

namespace ClaimHub.Domain.Models;

public enum ClaimAttentionLevel
{
    None = 0,
    Watch = 1,
    Action = 2
}

public sealed record ClaimAttentionItem(ClaimAttentionLevel Level, string Message);

public sealed class ClaimDecisionSummary
{
    public int ClaimId { get; init; }

    public string ClaimNumber { get; init; } = string.Empty;

    public ClaimStatus Status { get; init; }

    public required ClaimEligibilityResult Eligibility { get; init; }

    public required FraudAssessment Fraud { get; init; }

    public required ClaimFinancialSummary Financial { get; init; }

    public required SlaAssessment Sla { get; init; }

    public IReadOnlyList<ClaimAttentionItem> Attention { get; init; } = Array.Empty<ClaimAttentionItem>();

    public ClaimAttentionLevel Level => Attention.Count == 0
        ? ClaimAttentionLevel.None
        : Attention.Max(a => a.Level);

    public bool NeedsEscalation => Sla.RequiresEscalation;

    public IEnumerable<string> Messages => Attention.Select(a => a.Message);
}

public sealed class ClaimQueueSummary
{
    public int OpenCount { get; init; }

    public int EscalationCount { get; init; }

    public int FlaggedCount { get; init; }

    public int LateReportedCount { get; init; }

    public decimal TotalReserve { get; init; }

    public decimal TotalPaid { get; init; }

    public IReadOnlyDictionary<ClaimStatus, int> ByStatus { get; init; } = new Dictionary<ClaimStatus, int>();

    public decimal TotalExposure => TotalReserve + TotalPaid;
}