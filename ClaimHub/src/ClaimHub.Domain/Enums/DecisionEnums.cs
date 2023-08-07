namespace ClaimHub.Domain.Enums;

public enum ClaimTimelineEventKind
{
    Reported = 1,
    StatusChanged = 2,
    DocumentUploaded = 3,
    FraudFlagRaised = 4,
    Escalated = 5,
    PaidOut = 6
}
