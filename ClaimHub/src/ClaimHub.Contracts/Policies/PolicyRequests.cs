using System.ComponentModel.DataAnnotations;
using ClaimHub.Domain.Enums;

namespace ClaimHub.Contracts.Policies;

public class HomeRiskInput
{
    [Required, StringLength(120)]
    public string Street { get; set; } = string.Empty;

    [Required, StringLength(4, MinimumLength = 4)]
    public string PostalCode { get; set; } = string.Empty;

    [Required, StringLength(80)]
    public string City { get; set; } = string.Empty;

    [Range(1, 4)]
    public int PostcodeZone { get; set; } = 1;

    [Range(10_000, 5_000_000)]
    public decimal BuildingValue { get; set; }

    [Range(0, 1_000_000)]
    public decimal ContentsValue { get; set; }

    [Range(1800, 2100)]
    public int ConstructionYear { get; set; }

    public bool IsOwnerOccupied { get; set; } = true;
}

public class MotorRiskInput
{
    [Required, StringLength(12)]
    public string LicensePlate { get; set; } = string.Empty;

    [Required, StringLength(60)]
    public string Make { get; set; } = string.Empty;

    [Required, StringLength(60)]
    public string Model { get; set; } = string.Empty;

    [Range(1980, 2100)]
    public int FirstRegistrationYear { get; set; }

    [Range(500, 1_000_000)]
    public decimal VehicleValue { get; set; }

    [Range(1, 1000)]
    public int PowerKw { get; set; }

    [Required]
    public DateOnly DriverDateOfBirth { get; set; }

    [Range(0, 60)]
    public int ClaimFreeYears { get; set; }
}

public class TravelRiskInput
{
    public TravelScope Scope { get; set; } = TravelScope.Europe;

    public TravelPartyType PartyType { get; set; } = TravelPartyType.Single;
}

public class InsuredPersonInput
{
    [Required, StringLength(80)]
    public string FirstName { get; set; } = string.Empty;

    [Required, StringLength(80)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    public DateOnly DateOfBirth { get; set; }

    public string? NationalNumber { get; set; }

    public InsuredRelationship Relationship { get; set; } = InsuredRelationship.Other;
}

public class QuotePolicyRequest
{
    [Range(1, int.MaxValue)]
    public int PolicyHolderId { get; set; }

    public ProductType ProductType { get; set; }

    [Required]
    public DateOnly StartDate { get; set; }

    public PaymentFrequency PaymentFrequency { get; set; } = PaymentFrequency.Annual;

    public List<CoverageType> Coverages { get; set; } = new();

    public HomeRiskInput? Home { get; set; }

    public MotorRiskInput? Motor { get; set; }

    public TravelRiskInput? Travel { get; set; }
}

public class IssuePolicyRequest : QuotePolicyRequest
{
    public List<InsuredPersonInput> InsuredPersons { get; set; } = new();
}

public class EndorsePolicyRequest
{
    public EndorsementType Type { get; set; }

    [Required]
    public DateOnly EffectiveDate { get; set; }

    [Required, StringLength(250)]
    public string Description { get; set; } = string.Empty;

    public PaymentFrequency? NewPaymentFrequency { get; set; }

    public List<CoverageType>? NewCoverages { get; set; }

    [Range(10_000, 5_000_000)]
    public decimal? NewBuildingValue { get; set; }

    [Range(0, 1_000_000)]
    public decimal? NewContentsValue { get; set; }

    [Range(500, 1_000_000)]
    public decimal? NewVehicleValue { get; set; }

    public string? NewStreet { get; set; }

    public string? NewPostalCode { get; set; }

    public string? NewCity { get; set; }
}

public class CancelPolicyRequest
{
    public DateOnly? EffectiveDate { get; set; }

    [Required, StringLength(250)]
    public string Reason { get; set; } = string.Empty;
}

public class RenewPolicyRequest
{
    public PaymentFrequency? NewPaymentFrequency { get; set; }

    [Range(0, 60)]
    public int? ClaimFreeYears { get; set; }
}

public class PolicySearchQuery
{
    public PolicyStatus? Status { get; set; }

    public ProductType? Product { get; set; }

    [Range(1, 10_000)]
    public int Page { get; set; } = 1;

    [Range(1, 200)]
    public int PageSize { get; set; } = 25;
}
