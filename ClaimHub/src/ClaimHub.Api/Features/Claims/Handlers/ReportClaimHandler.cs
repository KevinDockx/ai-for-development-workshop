using ClaimHub.Contracts.Claims;
using ClaimHub.Domain.Common;
using ClaimHub.Domain.Entities;
using ClaimHub.Domain.Rules;
using ClaimHub.Domain.Services;
using ClaimHub.Infrastructure.Data;
using ClaimHub.Infrastructure.Mediator;
using Microsoft.EntityFrameworkCore;

namespace ClaimHub.Api.Features.Claims.Handlers;

public class ReportClaimHandler : IRequestHandler<ReportClaimCommand, Result<ClaimDetailDto>>
{
    private readonly ClaimHubDbContext _db;
    private readonly ClaimEligibilityService _eligibility;
    private readonly ClaimInputValidator _inputValidator;
    private readonly ClaimNumberGenerator _numbers;
    private readonly FraudDetectionService _fraud;
    private readonly ClaimDetailBuilder _details;

    public ReportClaimHandler(
        ClaimHubDbContext db,
        ClaimEligibilityService eligibility,
        ClaimInputValidator inputValidator,
        ClaimNumberGenerator numbers,
        FraudDetectionService fraud,
        ClaimDetailBuilder details)
    {
        _db = db;
        _eligibility = eligibility;
        _inputValidator = inputValidator;
        _numbers = numbers;
        _fraud = fraud;
        _details = details;
    }

    public async Task<Result<ClaimDetailDto>> HandleAsync(ReportClaimCommand request, CancellationToken cancellationToken)
    {
        var policy = await _db.Policies
            .Include(p => p.Coverages)
            .Include(p => p.Claims)
            .FirstOrDefaultAsync(p => p.Id == request.PolicyId && !p.IsDeleted, cancellationToken);
        if (policy == null)
        {
            return Result.Failure<ClaimDetailDto>($"Policy {request.PolicyId} was not found.", "policy.not_found");
        }

        var now = DateTimeOffset.UtcNow;
        var eligibility = _eligibility.Evaluate(policy, request.DateOfLoss, now, request.CoverageType);
        if (!eligibility.IsEligible)
        {
            return Result.Failure<ClaimDetailDto>(string.Join(" ", eligibility.Reasons), "claim.not_eligible");
        }

        var coverage = request.CoverageType.HasValue ? policy.FindCoverage(request.CoverageType.Value) : null;
        var claim = new Claim
        {
            ClaimNumber = await _numbers.GenerateAsync(now.Year, cancellationToken),
            PolicyId = policy.Id,
            CoverageId = coverage?.Id,
            Status = Domain.Enums.ClaimStatus.Reported,
            DateOfLoss = request.DateOfLoss,
            ReportedAt = now,
            Description = request.Description.Trim(),
            IsLateReported = eligibility.IsLateReported,
            CreatedAt = now
        };

        var report = _inputValidator.Validate(claim, policy, DateOnly.FromDateTime(now.UtcDateTime));
        if (!report.IsValid)
        {
            return Result.Failure<ClaimDetailDto>(string.Join(" ", report.Issues.Select(i => i.Message)), "claim.invalid");
        }

        claim.StatusHistory.Add(new ClaimStatusHistory
        {
            FromStatus = claim.Status,
            ToStatus = claim.Status,
            ChangedAt = now,
            ChangedBy = request.Actor.Email,
            Note = "Reported"
        });

        var holderClaims = await _db.Claims
            .Where(c => c.Policy!.PolicyHolderId == policy.PolicyHolderId && !c.IsDeleted)
            .ToListAsync(cancellationToken);
        var assessment = _fraud.Assess(claim, policy, coverage, holderClaims);
        foreach (var flag in assessment.Flags)
        {
            claim.FraudFlags.Add(flag);
        }

        _db.Claims.Add(claim);
        await _db.SaveChangesAsync(cancellationToken);

        var saved = await _details.LoadAsync(claim.Id, cancellationToken);
        return Result.Success(_details.Build(saved!));
    }
}
