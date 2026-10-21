using ClaimHub.Contracts.Claims;
using ClaimHub.Domain.Common;
using ClaimHub.Domain.Entities;
using ClaimHub.Domain.Enums;
using ClaimHub.Domain.Rules;
using ClaimHub.Domain.Services;
using ClaimHub.Infrastructure.Data;
using ClaimHub.Infrastructure.Mediator;

namespace ClaimHub.Api.Features.Claims.Handlers;

public abstract class ClaimWorkflowHandler
{
    protected ClaimWorkflowHandler(ClaimHubDbContext db, ClaimStateMachine stateMachine, ClaimDetailBuilder details)
    {
        Db = db;
        StateMachine = stateMachine;
        Details = details;
    }

    protected ClaimHubDbContext Db { get; }

    protected ClaimStateMachine StateMachine { get; }

    protected ClaimDetailBuilder Details { get; }

    protected async Task<Result<ClaimDetailDto>> TransitionAsync(
        int claimId,
        ClaimStatus target,
        ClaimActor actor,
        string? note,
        Action<Claim>? beforeApply,
        CancellationToken cancellationToken)
    {
        var claim = await Details.LoadAsync(claimId, cancellationToken);
        if (claim == null)
        {
            return Result.Failure<ClaimDetailDto>($"Claim {claimId} was not found.", "claim.not_found");
        }

        var validation = StateMachine.Validate(claim, target);
        if (!validation.IsAllowed)
        {
            return Result.Failure<ClaimDetailDto>(validation.Reason ?? "Transition not allowed.", "claim.invalid_transition");
        }

        beforeApply?.Invoke(claim);
        StateMachine.Apply(claim, target, actor.Email, DateTimeOffset.UtcNow, note);
        await Db.SaveChangesAsync(cancellationToken);
        return Result.Success(Details.Build(claim));
    }
}

public class StartAssessmentHandler : ClaimWorkflowHandler, IRequestHandler<StartAssessmentCommand, Result<ClaimDetailDto>>
{
    public StartAssessmentHandler(ClaimHubDbContext db, ClaimStateMachine stateMachine, ClaimDetailBuilder details)
        : base(db, stateMachine, details)
    {
    }

    public Task<Result<ClaimDetailDto>> HandleAsync(StartAssessmentCommand request, CancellationToken cancellationToken) =>
        TransitionAsync(
            request.ClaimId,
            ClaimStatus.UnderAssessment,
            request.Actor,
            "Assessment started",
            claim => claim.AssignedHandlerId = request.Actor.UserId,
            cancellationToken);
}

public class AssessClaimHandler : IRequestHandler<AssessClaimCommand, Result<ClaimDetailDto>>
{
    private readonly ClaimHubDbContext _db;
    private readonly ClaimInputValidator _inputValidator;
    private readonly ClaimDetailBuilder _details;

    public AssessClaimHandler(ClaimHubDbContext db, ClaimInputValidator inputValidator, ClaimDetailBuilder details)
    {
        _db = db;
        _inputValidator = inputValidator;
        _details = details;
    }

    public async Task<Result<ClaimDetailDto>> HandleAsync(AssessClaimCommand request, CancellationToken cancellationToken)
    {
        var claim = await _details.LoadAsync(request.ClaimId, cancellationToken);
        if (claim == null)
        {
            return Result.Failure<ClaimDetailDto>($"Claim {request.ClaimId} was not found.", "claim.not_found");
        }

        if (claim.Status != ClaimStatus.UnderAssessment)
        {
            return Result.Failure<ClaimDetailDto>("Only a claim under assessment can be assessed.", "claim.invalid_transition");
        }

        claim.AssessedAmount = request.AssessedAmount;
        var report = _inputValidator.ValidateAmounts(claim);
        if (!report.IsValid)
        {
            return Result.Failure<ClaimDetailDto>(string.Join(" ", report.Issues.Select(i => i.Message)), "claim.invalid");
        }

        claim.RegisterAssessment(DateTimeOffset.UtcNow);
        claim.StatusHistory.Add(new ClaimStatusHistory
        {
            ClaimId = claim.Id,
            FromStatus = claim.Status,
            ToStatus = claim.Status,
            ChangedAt = DateTimeOffset.UtcNow,
            ChangedBy = request.Actor.Email,
            Note = request.Note ?? $"Assessed at {request.AssessedAmount:0.00}"
        });

        await _db.SaveChangesAsync(cancellationToken);
        return Result.Success(_details.Build(claim));
    }
}

public class ApproveClaimHandler : ClaimWorkflowHandler, IRequestHandler<ApproveClaimCommand, Result<ClaimDetailDto>>
{
    private readonly ClaimReserveService _reserves;

    public ApproveClaimHandler(
        ClaimHubDbContext db,
        ClaimStateMachine stateMachine,
        ClaimDetailBuilder details,
        ClaimReserveService reserves)
        : base(db, stateMachine, details)
    {
        _reserves = reserves;
    }

    public Task<Result<ClaimDetailDto>> HandleAsync(ApproveClaimCommand request, CancellationToken cancellationToken) =>
        TransitionAsync(
            request.ClaimId,
            ClaimStatus.Approved,
            request.Actor,
            request.Note ?? "Approved",
            claim =>
            {
                if (claim.AssessedAmount.HasValue && claim.Coverage != null && claim.Policy != null)
                {
                    claim.ApprovedAmount = _reserves.Summarize(claim, claim.Policy).ExpectedPayout;
                }
            },
            cancellationToken);
}

public class RejectClaimHandler : ClaimWorkflowHandler, IRequestHandler<RejectClaimCommand, Result<ClaimDetailDto>>
{
    public RejectClaimHandler(ClaimHubDbContext db, ClaimStateMachine stateMachine, ClaimDetailBuilder details)
        : base(db, stateMachine, details)
    {
    }

    public Task<Result<ClaimDetailDto>> HandleAsync(RejectClaimCommand request, CancellationToken cancellationToken) =>
        TransitionAsync(request.ClaimId, ClaimStatus.Rejected, request.Actor, request.Reason, null, cancellationToken);
}

public class WithdrawClaimHandler : ClaimWorkflowHandler, IRequestHandler<WithdrawClaimCommand, Result<ClaimDetailDto>>
{
    public WithdrawClaimHandler(ClaimHubDbContext db, ClaimStateMachine stateMachine, ClaimDetailBuilder details)
        : base(db, stateMachine, details)
    {
    }

    public Task<Result<ClaimDetailDto>> HandleAsync(WithdrawClaimCommand request, CancellationToken cancellationToken) =>
        TransitionAsync(request.ClaimId, ClaimStatus.Withdrawn, request.Actor, request.Reason ?? "Withdrawn", null, cancellationToken);
}

public class CloseClaimHandler : ClaimWorkflowHandler, IRequestHandler<CloseClaimCommand, Result<ClaimDetailDto>>
{
    public CloseClaimHandler(ClaimHubDbContext db, ClaimStateMachine stateMachine, ClaimDetailBuilder details)
        : base(db, stateMachine, details)
    {
    }

    public Task<Result<ClaimDetailDto>> HandleAsync(CloseClaimCommand request, CancellationToken cancellationToken) =>
        TransitionAsync(request.ClaimId, ClaimStatus.Closed, request.Actor, "Closed", null, cancellationToken);
}
