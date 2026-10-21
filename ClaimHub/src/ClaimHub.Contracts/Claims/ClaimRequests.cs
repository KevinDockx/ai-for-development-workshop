using ClaimHub.Domain.Enums;

namespace ClaimHub.Contracts.Claims;

public record ReportClaimBody(int PolicyId, CoverageType? CoverageType, DateOnly DateOfLoss, string Description);

public record AssessClaimBody(decimal AssessedAmount, string? Note);

public record ApproveClaimBody(string? Note);

public record RejectClaimBody(string Reason);

public record WithdrawClaimBody(string? Reason);

public record ListClaimsParameters(int Page = 1, int PageSize = 20, ClaimStatus? Status = null, int? PolicyId = null);