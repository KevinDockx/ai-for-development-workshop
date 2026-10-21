using ClaimHub.Domain.Entities;
using ClaimHub.Domain.Enums;
using ClaimHub.Domain.Models;

namespace ClaimHub.Domain.Rules;

public static class ProductCoverageRules
{
    public static IReadOnlyList<CoverageType> AllowedFor(ProductType product) => product switch
    {
        ProductType.Home => new[] { CoverageType.Building, CoverageType.Contents },
        ProductType.Motor => new[] { CoverageType.ThirdPartyLiability, CoverageType.Comprehensive, CoverageType.Glass },
        ProductType.Travel => new[] { CoverageType.Travel },
        _ => Array.Empty<CoverageType>()
    };

    public static ValidationReport Validate(ProductType product, IEnumerable<CoverageType> selected)
    {
        ArgumentNullException.ThrowIfNull(selected);
        var report = new ValidationReport();
        var types = selected.ToList();

        foreach (var type in types.Where(t => !Coverage.IsAllowedFor(product, t)).Distinct())
        {
            report.Add("Coverages", $"{type} cannot be insured on a {product} policy.");
        }

        if (product is ProductType.Home or ProductType.Travel && types.Count == 0)
        {
            report.Add("Coverages", $"At least one coverage must be selected for a {product} policy.");
        }

        if (product == ProductType.Motor && !types.Contains(CoverageType.ThirdPartyLiability))
        {
            report.Add("Coverages", "ThirdPartyLiability is mandatory for a Motor policy.");
        }

        return report;
    }

    public static ValidationReport Validate(Policy policy)
    {
        ArgumentNullException.ThrowIfNull(policy);
        return Validate(policy.ProductType, policy.Coverages.Where(c => c.IsActive).Select(c => c.Type));
    }
}