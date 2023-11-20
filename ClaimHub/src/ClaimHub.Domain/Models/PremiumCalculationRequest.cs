using ClaimHub.Domain.Enums;

namespace ClaimHub.Domain.Models;

public class PremiumCalculationRequest
{
    public const decimal DefaultTaxRate = 0.0925m;

    public ProductType ProductType { get; set; }

    public PaymentFrequency PaymentFrequency { get; set; } = PaymentFrequency.Annual;

    public DateOnly StartDate { get; set; }

    public IList<CoverageType> Coverages { get; set; } = new List<CoverageType>();

    public decimal TaxRate { get; set; } = DefaultTaxRate;

    // Home
    public int PostcodeZone { get; set; } = 1;

    public decimal BuildingValue { get; set; }

    public decimal ContentsValue { get; set; }

    public int ConstructionYear { get; set; }

    // Motor
    public int DriverAge { get; set; }

    public decimal VehicleValue { get; set; }

    public int VehiclePowerKw { get; set; }

    // Travel
    public TravelScope TravelScope { get; set; } = TravelScope.Europe;

    public TravelPartyType PartyType { get; set; } = TravelPartyType.Single;

    // Discount inputs
    public int ClaimFreeYears { get; set; }

    public int ActivePolicyCount { get; set; }

    public int FullYearsAsCustomer { get; set; }
}
