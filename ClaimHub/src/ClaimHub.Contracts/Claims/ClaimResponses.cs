using ClaimHub.Domain.Enums;

namespace ClaimHub.Contracts.Claims;

public record ClaimDto(
    int Id,
    string ClaimNumber,
    int PolicyId,
    string? PolicyNumber,
    ClaimStatus Status,
    DateOnly DateOfLoss,
    DateTimeOffset ReportedAt,
    string Description,
    bool IsLateReported,
    decimal? AssessedAmount,
    decimal? ApprovedAmount,
    decimal PaidAmount);

public record ClaimStatusHistoryDto(ClaimStatus From, ClaimStatus To, DateTimeOffset ChangedAt, string ChangedBy, string? Note);

public record ClaimTimelineEntryDto(DateTimeOffset At, string Kind, string Title, string? Detail);

public record ClaimDocumentDto(int Id, string FileName, DocumentType Type, long SizeBytes, DateTimeOffset UploadedAt);

public record FraudFlagDto(FraudFlagType Type, string Details, int Weight, bool IsReviewed);

public record ClaimFinancialsDto(
    decimal Deductible,
    decimal CoverageLimit,
    decimal RemainingLimit,
    decimal ExpectedPayout,
    decimal Reserve,
    bool LimitReached);

public record ClaimDetailDto(
    ClaimDto Claim,
    CoverageType? CoverageType,
    int? AssignedHandlerId,
    IReadOnlyList<ClaimStatusHistoryDto> History,
    IReadOnlyList<ClaimTimelineEntryDto> Timeline,
    IReadOnlyList<ClaimDocumentDto> Documents,
    IReadOnlyList<FraudFlagDto> FraudFlags,
    ClaimFinancialsDto Financials);

public record ClaimPageDto(IReadOnlyList<ClaimDto> Items, int Page, int PageSize, int Total);

public record PayoutDto(int ClaimId, string ClaimNumber, decimal Amount, string? GatewayReference, ClaimStatus Status);
