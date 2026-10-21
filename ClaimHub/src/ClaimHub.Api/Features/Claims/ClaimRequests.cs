using ClaimHub.Contracts.Claims;
using ClaimHub.Domain.Common;
using ClaimHub.Domain.Enums;
using ClaimHub.Infrastructure.Mediator;

namespace ClaimHub.Api.Features.Claims;

public record ClaimActor(int UserId, string Email, UserRole? Role, int? PolicyHolderId, int? BrokerId);

public record ReportClaimCommand(ClaimActor Actor, int PolicyId, CoverageType? CoverageType, DateOnly DateOfLoss, string Description)
    : IRequest<Result<ClaimDetailDto>>;

public record StartAssessmentCommand(ClaimActor Actor, int ClaimId) : IRequest<Result<ClaimDetailDto>>;

public record AssessClaimCommand(ClaimActor Actor, int ClaimId, decimal AssessedAmount, string? Note) : IRequest<Result<ClaimDetailDto>>;

public record ApproveClaimCommand(ClaimActor Actor, int ClaimId, string? Note) : IRequest<Result<ClaimDetailDto>>;

public record RejectClaimCommand(ClaimActor Actor, int ClaimId, string Reason) : IRequest<Result<ClaimDetailDto>>;

public record PayOutClaimCommand(ClaimActor Actor, int ClaimId) : IRequest<Result<PayoutDto>>;

public record WithdrawClaimCommand(ClaimActor Actor, int ClaimId, string? Reason) : IRequest<Result<ClaimDetailDto>>;

public record CloseClaimCommand(ClaimActor Actor, int ClaimId) : IRequest<Result<ClaimDetailDto>>;

public record GetClaimQuery(int ClaimId) : IRequest<Result<ClaimDetailDto>>;

public record ListClaimsQuery(int Page, int PageSize, ClaimStatus? Status, int? PolicyId) : IRequest<ClaimPageDto>;

public record SearchClaimsQuery(string Term) : IRequest<IReadOnlyList<ClaimDto>>;
