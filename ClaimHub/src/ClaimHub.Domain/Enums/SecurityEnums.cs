namespace ClaimHub.Domain.Enums;

public enum UserRole
{
    Admin = 1,
    Underwriter = 2,
    ClaimsHandler = 3,
    SeniorClaimsHandler = 4,
    Broker = 5,
    PolicyHolder = 6
}

public enum AuditAction
{
    Created = 1,
    Updated = 2,
    Deleted = 3,
    StatusChanged = 4,
    Viewed = 5,
    Exported = 6
}
