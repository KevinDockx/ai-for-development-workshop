using ClaimHub.Domain.Enums;
using ClaimHub.Domain.Rules;

namespace ClaimHub.Domain.Tests.Validation;

public class ProductCoverageRulesTests
{
    [Fact]
    public void Should_List_Allowed_Coverages_Per_Product()
    {
        Assert.Equal(2, ProductCoverageRules.AllowedFor(ProductType.Home).Count);
        Assert.Equal(3, ProductCoverageRules.AllowedFor(ProductType.Motor).Count);
        Assert.Single(ProductCoverageRules.AllowedFor(ProductType.Travel));
    }

    [Fact]
    public void Should_Accept_Motor_With_Liability_And_Glass()
    {
        var report = ProductCoverageRules.Validate(
            ProductType.Motor, new[] { CoverageType.ThirdPartyLiability, CoverageType.Glass });

        Assert.True(report.IsValid);
    }

    [Fact]
    public void Should_Report_Each_Foreign_Coverage_Once()
    {
        var report = ProductCoverageRules.Validate(
            ProductType.Travel, new[] { CoverageType.Travel, CoverageType.Glass, CoverageType.Glass });

        Assert.Single(report.Issues);
    }

    [Fact]
    public void Should_Require_At_Least_One_Home_Coverage()
    {
        var report = ProductCoverageRules.Validate(ProductType.Home, Array.Empty<CoverageType>());

        Assert.False(report.IsValid);
    }
}
