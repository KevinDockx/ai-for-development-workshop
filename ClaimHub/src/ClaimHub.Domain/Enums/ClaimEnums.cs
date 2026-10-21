namespace ClaimHub.Domain.Enums;

public enum ClaimStatus
{
    Reported = 0,
    UnderAssessment = 1,
    Approved = 2,
    Rejected = 3,
    PaidOut = 4,
    Closed = 5,
    Withdrawn = 6
}

public enum FraudFlagType
{
    FrequentClaims = 1,
    LossShortlyAfterStart = 2,
    HighAmountComparedToLimit = 3
}

public enum DocumentType
{
    ClaimForm = 1,
    Photo = 2,
    Invoice = 3,
    PoliceReport = 4,
    MedicalCertificate = 5,
    Other = 9
}

public enum FraudRiskLevel
{
    None = 0,
    Low = 1,
    Medium = 2,
    High = 3
}
