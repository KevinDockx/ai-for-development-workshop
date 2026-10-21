namespace ClaimHub.Domain.Enums;

public enum ProductType
{
    Home = 1,
    Motor = 2,
    Travel = 3
}

public enum PolicyStatus
{
    Quoted = 0,
    Active = 1,
    Cancelled = 2,
    Expired = 3,
    Renewed = 4,
    Suspended = 5
}

public enum CoverageType
{
    Building = 1,
    Contents = 2,
    ThirdPartyLiability = 3,
    Comprehensive = 4,
    Glass = 5,
    Travel = 6
}

public enum PaymentFrequency
{
    Annual = 1,
    Quarterly = 2,
    Monthly = 3
}

public enum EndorsementType
{
    CoverageChange = 1,
    AddressChange = 2,
    VehicleChange = 3,
    ValueAdjustment = 4,
    InsuredPersonChange = 5,
    PaymentFrequencyChange = 6
}

public enum TravelScope
{
    Europe = 1,
    Worldwide = 2
}

public enum TravelPartyType
{
    Single = 1,
    Family = 2
}

public enum InsuredRelationship
{
    Self = 0,
    Partner = 1,
    Child = 2,
    Parent = 3,
    Other = 4
}
