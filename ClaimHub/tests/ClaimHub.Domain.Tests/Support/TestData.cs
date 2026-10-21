using ClaimHub.Domain.Entities;
using ClaimHub.Domain.Enums;
using ClaimHub.Domain.Models;

namespace ClaimHub.Domain.Tests.Support;

internal static class TestData
{
    public static readonly DateOnly NewYear2025 = new(2025, 1, 1);

    public static PremiumCalculationRequest HomeRequest(
        decimal buildingValue = 250_000m,
        decimal contentsValue = 0m,
        int zone = 2,
        int constructionYear = 2000,
        params CoverageType[] coverages)
    {
        return new PremiumCalculationRequest
        {
            ProductType = ProductType.Home,
            StartDate = NewYear2025,
            PostcodeZone = zone,
            BuildingValue = buildingValue,
            ContentsValue = contentsValue,
            ConstructionYear = constructionYear,
            Coverages = coverages.Length == 0 ? new List<CoverageType> { CoverageType.Building } : coverages.ToList()
        };
    }

    public static PremiumCalculationRequest MotorRequest(int driverAge = 40, int powerKw = 80, decimal vehicleValue = 20_000m)
    {
        return new PremiumCalculationRequest
        {
            ProductType = ProductType.Motor,
            StartDate = NewYear2025,
            DriverAge = driverAge,
            VehiclePowerKw = powerKw,
            VehicleValue = vehicleValue,
            Coverages = new List<CoverageType> { CoverageType.ThirdPartyLiability }
        };
    }

    public static PremiumCalculationRequest TravelRequest(TravelPartyType party, TravelScope scope)
    {
        return new PremiumCalculationRequest
        {
            ProductType = ProductType.Travel,
            StartDate = NewYear2025,
            PartyType = party,
            TravelScope = scope,
            Coverages = new List<CoverageType> { CoverageType.Travel }
        };
    }

    public static PolicyHolder PolicyHolder(string first = "Els", string last = "Vermeulen", DateOnly? customerSince = null)
    {
        return new PolicyHolder
        {
            Id = 1,
            FirstName = first,
            LastName = last,
            Email = "els.vermeulen@example.test",
            CustomerSince = customerSince ?? new DateOnly(2015, 6, 15)
        };
    }
}

internal sealed class PolicyBuilder
{
    private readonly Policy _policy = new()
    {
        Id = 10,
        PolicyHolderId = 1,
        PolicyNumber = "HOM-2025-000010",
        ProductType = ProductType.Home,
        StartDate = new DateOnly(2025, 1, 1),
        EndDate = new DateOnly(2025, 12, 31),
        AnnualPremium = 600m,
        PaymentFrequency = PaymentFrequency.Annual
    };

    public PolicyBuilder()
    {
        _policy.Coverages.Add(new Coverage { Type = CoverageType.Building, Limit = 300_000m, Deductible = 250m });
        _policy.HomeRisk = new HomeRisk
        {
            Street = "Meir 12",
            PostalCode = "2000",
            City = "Antwerpen",
            PostcodeZone = 2,
            BuildingValue = 250_000m,
            ConstructionYear = 1998
        };
    }

    public PolicyBuilder WithStatus(PolicyStatus status)
    {
        _policy.Status = status;
        return this;
    }

    public PolicyBuilder WithTerm(DateOnly start, DateOnly end)
    {
        _policy.StartDate = start;
        _policy.EndDate = end;
        return this;
    }

    public PolicyBuilder WithAnnualPremium(decimal premium)
    {
        _policy.AnnualPremium = premium;
        return this;
    }

    public PolicyBuilder AsMotor(bool withLiability = true)
    {
        _policy.ProductType = ProductType.Motor;
        _policy.HomeRisk = null;
        _policy.Coverages.Clear();
        _policy.Coverages.Add(withLiability
            ? new Coverage { Type = CoverageType.ThirdPartyLiability, Limit = 1_000_000m }
            : new Coverage { Type = CoverageType.Glass, Limit = 500m });

        _policy.MotorRisk = new MotorRisk
        {
            LicensePlate = "1-ABC-123",
            Make = "Skoda",
            Model = "Octavia",
            VehicleValue = 24_000m,
            PowerKw = 85
        };
        return this;
    }

    public Policy Build() => _policy;
}
