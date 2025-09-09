using ClaimHub.Domain.Enums;

namespace ClaimHub.Domain.Models;

public enum PremiumDirection
{
    NoChange = 0,
    Additional = 1,
    Credit = 2
}

public sealed class CancellationDecisionSummary
{
    public int PolicyId { get; init; }

    public string PolicyNumber { get; init; } = string.Empty;

    public required CancellationRefundResult Refund { get; init; }

    public int PaidClaimCount { get; init; }

    public decimal PaidClaimsInTerm { get; init; }

    public IReadOnlyList<int> InvoiceIdsToCancel { get; init; } = Array.Empty<int>();

    public decimal OutstandingOnOpenInvoices { get; init; }

    public bool HasPaidClaimsInTerm => PaidClaimCount > 0;

    public string Headline =>
        Refund.HasRefund
            ? $"Policy {PolicyNumber}: refund {Refund.RefundAmount:0.00} for {Refund.UnusedDays} unused days of {Refund.TermDays}."
            : $"Policy {PolicyNumber}: no refund ({Refund.Message}).";
}

public sealed class EndorsementDecisionSummary
{
    public int PolicyId { get; init; }

    public string PolicyNumber { get; init; } = string.Empty;

    public EndorsementType Type { get; init; }

    public required EndorsementPremiumResult Premium { get; init; }

    public PremiumDirection Direction { get; init; }

    public decimal RemainingShare { get; init; }

    public bool NeedsInvoice => Direction == PremiumDirection.Additional;

    public string Headline => Direction switch
    {
        PremiumDirection.Additional => $"{PolicyNumber}: additional premium {Premium.AdditionalPremium:0.00} for {Premium.RemainingDays} remaining days.",
        PremiumDirection.Credit => $"{PolicyNumber}: credit {-Premium.AdditionalPremium:0.00} for {Premium.RemainingDays} remaining days.",
        _ => $"{PolicyNumber}: the endorsement does not change the premium."
    };
}