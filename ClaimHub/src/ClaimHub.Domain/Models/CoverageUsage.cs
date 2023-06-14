using ClaimHub.Domain.Enums;

namespace ClaimHub.Domain.Models;

public class CoverageUsage
{
    public int CoverageId { get; set; }

    public CoverageType Type { get; set; }

    public decimal Limit { get; set; }

    public decimal PaidInTerm { get; set; }

    public decimal Remaining => Limit - PaidInTerm > 0m ? Limit - PaidInTerm : 0m;

    public decimal UsedPercentage => Limit <= 0m ? 0m : Math.Round(PaidInTerm / Limit * 100m, 1);
}
