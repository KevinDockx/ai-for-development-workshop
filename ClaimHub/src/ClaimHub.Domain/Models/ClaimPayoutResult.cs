namespace ClaimHub.Domain.Models;

public class ClaimPayoutResult
{
    public decimal AssessedAmount { get; set; }

    public decimal Deductible { get; set; }

    public decimal CoverageLimit { get; set; }

    public decimal RemainingLimit { get; set; }

    public decimal PayoutAmount { get; set; }

    public bool LimitReached { get; set; }

    public bool HasPayout => PayoutAmount > 0m;
}
