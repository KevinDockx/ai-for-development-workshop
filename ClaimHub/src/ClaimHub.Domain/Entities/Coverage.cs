using ClaimHub.Domain.Enums;

namespace ClaimHub.Domain.Entities;

public class Coverage
{
    public int Id { get; set; }

    public int PolicyId { get; set; }

    public virtual Policy? Policy { get; set; }

    public CoverageType Type { get; set; }

    public decimal Limit { get; set; }

    public decimal Deductible { get; set; }

    public decimal Premium { get; set; }

    public bool IsActive { get; set; } = true;

    public virtual ICollection<Claim> Claims { get; set; } = new List<Claim>();

    public bool IsMandatoryFor(ProductType product) =>
        product == ProductType.Motor && Type == CoverageType.ThirdPartyLiability;

    public static bool IsAllowedFor(ProductType product, CoverageType type) => product switch
    {
        ProductType.Home => type is CoverageType.Building or CoverageType.Contents,
        ProductType.Motor => type is CoverageType.ThirdPartyLiability or CoverageType.Comprehensive or CoverageType.Glass,
        ProductType.Travel => type is CoverageType.Travel,
        _ => false
    };
}
