using ClaimHub.Contracts.Claims;
using ClaimHub.Domain.Common;
using ClaimHub.Domain.Entities;
using ClaimHub.Domain.Services;
using ClaimHub.Infrastructure.Data;
using ClaimHub.Infrastructure.Mediator;
using Microsoft.EntityFrameworkCore;

namespace ClaimHub.Api.Features.Claims.Handlers;

public class ClaimDetailBuilder
{
    private readonly ClaimHubDbContext _db;
    private readonly ClaimTimelineBuilder _timelines;
    private readonly ClaimReserveService _reserves;

    public ClaimDetailBuilder(ClaimHubDbContext db, ClaimTimelineBuilder timelines, ClaimReserveService reserves)
    {
        _db = db;
        _timelines = timelines;
        _reserves = reserves;
    }

    public Task<Claim?> LoadAsync(int claimId, CancellationToken cancellationToken) =>
        _db.Claims
            .Include(c => c.Policy).ThenInclude(p => p!.Claims)
            .Include(c => c.Policy).ThenInclude(p => p!.Coverages)
            .Include(c => c.Coverage)
            .Include(c => c.StatusHistory)
            .Include(c => c.Documents)
            .Include(c => c.FraudFlags)
            .FirstOrDefaultAsync(c => c.Id == claimId && !c.IsDeleted, cancellationToken);

    public ClaimDetailDto Build(Claim claim)
    {
        var financials = claim.Coverage != null && claim.Policy != null
            ? _reserves.Summarize(claim, claim.Policy).ToDto()
            : new ClaimFinancialsDto(0m, 0m, 0m, 0m, 0m, false);

        return new ClaimDetailDto(
            claim.ToDto(),
            claim.Coverage?.Type,
            claim.AssignedHandlerId,
            claim.StatusHistory.OrderBy(h => h.ChangedAt).Select(h => h.ToDto()).ToList(),
            _timelines.Build(claim).Select(e => e.ToDto()).ToList(),
            claim.Documents.Select(d => d.ToDto()).ToList(),
            claim.FraudFlags.Select(f => f.ToDto()).ToList(),
            financials);
    }
}

public class GetClaimHandler : IRequestHandler<GetClaimQuery, Result<ClaimDetailDto>>
{
    private readonly ClaimDetailBuilder _details;

    public GetClaimHandler(ClaimDetailBuilder details)
    {
        _details = details;
    }

    public async Task<Result<ClaimDetailDto>> HandleAsync(GetClaimQuery request, CancellationToken cancellationToken)
    {
        var claim = await _details.LoadAsync(request.ClaimId, cancellationToken);
        if (claim == null)
        {
            return Result.Failure<ClaimDetailDto>($"Claim {request.ClaimId} was not found.", "claim.not_found");
        }

        return Result.Success(_details.Build(claim));
    }
}

public class ListClaimsHandler : IRequestHandler<ListClaimsQuery, ClaimPageDto>
{
    private readonly ClaimHubDbContext _db;

    public ListClaimsHandler(ClaimHubDbContext db)
    {
        _db = db;
    }

    public async Task<ClaimPageDto> HandleAsync(ListClaimsQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Claims.AsNoTracking().Include(c => c.Policy).Where(c => !c.IsDeleted);
        if (request.Status.HasValue)
        {
            query = query.Where(c => c.Status == request.Status.Value);
        }

        if (request.PolicyId.HasValue)
        {
            query = query.Where(c => c.PolicyId == request.PolicyId.Value);
        }

        int total = await query.CountAsync(cancellationToken);
        var claims = await query
            .OrderBy(c => c.Id)
            .Skip(request.Page * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new ClaimPageDto(claims.Select(c => c.ToDto()).ToList(), request.Page, request.PageSize, total);
    }
}

public class SearchClaimsHandler : IRequestHandler<SearchClaimsQuery, IReadOnlyList<ClaimDto>>
{
    private readonly ClaimHubDbContext _db;

    public SearchClaimsHandler(ClaimHubDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<ClaimDto>> HandleAsync(SearchClaimsQuery request, CancellationToken cancellationToken)
    {
        var term = request.Term.Trim();
        var claims = await _db.Claims
            .FromSqlRaw($"SELECT * FROM Claims WHERE IsDeleted = 0 AND (ClaimNumber LIKE '%{term}%' OR Description LIKE '%{term}%')")
            .AsNoTracking()
            .OrderBy(c => c.ClaimNumber)
            .Take(50)
            .ToListAsync(cancellationToken);

        return claims.Select(c => c.ToDto()).ToList();
    }
}
