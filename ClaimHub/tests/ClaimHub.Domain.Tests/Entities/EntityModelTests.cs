using ClaimHub.Domain.Entities;
using ClaimHub.Domain.Enums;

namespace ClaimHub.Domain.Tests.Entities;

public class EntityModelTests
{
    [Theory]
    [InlineData(ProductType.Home, CoverageType.Building, true)]
    [InlineData(ProductType.Home, CoverageType.Glass, false)]
    [InlineData(ProductType.Motor, CoverageType.Comprehensive, true)]
    [InlineData(ProductType.Travel, CoverageType.Travel, true)]
    [InlineData(ProductType.Travel, CoverageType.Building, false)]
    public void Coverage_IsAllowedFor_MatchesProductCatalogue(ProductType product, CoverageType type, bool expected)
    {
        Assert.Equal(expected, Coverage.IsAllowedFor(product, type));
    }

    [Fact]
    public void Coverage_TpL_IsMandatoryForMotorOnly()
    {
        var coverage = new Coverage { Type = CoverageType.ThirdPartyLiability };

        Assert.True(coverage.IsMandatoryFor(ProductType.Motor));
        Assert.False(coverage.IsMandatoryFor(ProductType.Home));
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(4, true)]
    [InlineData(0, false)]
    [InlineData(5, false)]
    public void HomeRisk_Zone_MustBeBetweenOneAndFour(int zone, bool expected)
    {
        var risk = new HomeRisk { PostcodeZone = zone };

        Assert.Equal(expected, risk.HasValidZone());
    }

    [Fact]
    public void HomeRisk_BuildingAge_IsNeverNegative()
    {
        var risk = new HomeRisk { ConstructionYear = 2030 };

        Assert.Equal(0, risk.BuildingAgeIn(2025));
        Assert.Equal(25, new HomeRisk { ConstructionYear = 2000 }.BuildingAgeIn(2025));
    }

    [Fact]
    public void MotorRisk_Power_MustBePositive()
    {
        Assert.True(new MotorRisk { PowerKw = 74 }.HasValidPower());
        Assert.False(new MotorRisk { PowerKw = 0 }.HasValidPower());
    }
}
