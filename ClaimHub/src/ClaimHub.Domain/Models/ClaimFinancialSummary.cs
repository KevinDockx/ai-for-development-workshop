using ClaimHub.Domain.Enums;

namespace ClaimHub.Domain.Models;

public sealed class ClaimFinancialSummary
{
    public int ClaimId { get; init; }

    public ClaimStatus Status { get; init; }

    public decimal? AssessedAmount { get; init; }

    public decimal Deductible { get; init; }

    public decimal CoverageLimit { get; init; }

    public decimal PaidInTermByOthers { get; init; }

    public decimal RemainingLimit { get; init; }

    public decimal ExpectedPayout { get; init; }

    public decimal PaidAmount { get; init; }

    public decimal Reserve { get; init; }

    public bool LimitReached { get; init; }

    public decimal TotalExposure => PaidAmount + Reserve;
}
