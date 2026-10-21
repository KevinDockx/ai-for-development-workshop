using ClaimHub.Domain.Common;
using ClaimHub.Domain.Entities;
using ClaimHub.Domain.Enums;
using ClaimHub.Domain.Models;
using ClaimHub.Domain.Services;
using ClaimHub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ClaimHub.Infrastructure.Reporting;

/// <summary>Loads portfolio, claim queue and dunning data and hands it to the matching domain summary services.</summary>
public class PortfolioReadService
{
    private readonly ClaimHubDbContext _db;
    private readonly PolicyPortfolioService _portfolio;
    private readonly ClaimDecisionService _claims;
    private readonly DunningScheduleService _dunning;

    public PortfolioReadService(
        ClaimHubDbContext db,
        PolicyPortfolioService portfolio,
        ClaimDecisionService claims,
        DunningScheduleService dunning)
    {
        _db = db;
        _portfolio = portfolio;
        _claims = claims;
        _dunning = dunning;
    }

    public async Task<PolicyPortfolioSummary> GetPortfolioAsync(DateOnly asOf, CancellationToken cancellationToken)
    {
        var policies = await _db.Policies.AsNoTracking()
            .Where(p => !p.IsDeleted)
            .ToListAsync(cancellationToken);

        return _portfolio.Summarize(policies, asOf);
    }

    public async Task<IReadOnlyList<CoverageTermSummary>> GetCoveragesNearLimitAsync(CancellationToken cancellationToken)
    {
        var policies = await _db.Policies.AsNoTracking()
            .Include(p => p.Coverages)
            .Include(p => p.Claims)
            .Where(p => !p.IsDeleted && p.Status == PolicyStatus.Active)
            .ToListAsync(cancellationToken);

        return _portfolio.NearLimit(policies);
    }

    public async Task<ClaimQueueSummary> GetClaimQueueAsync(DateTimeOffset now, CancellationToken cancellationToken)
    {
        var claims = await _db.Claims.AsNoTrackingWithIdentityResolution()
            .Include(c => c.Coverage)
            .Include(c => c.FraudFlags)
            .Include(c => c.Policy).ThenInclude(p => p!.Coverages)
            .Include(c => c.Policy).ThenInclude(p => p!.Claims)
            .Where(c => !c.IsDeleted)
            .ToListAsync(cancellationToken);

        return _claims.SummarizeQueue(claims, now);
    }

    public async Task<DunningOverview> GetDunningOverviewAsync(DateOnly today, CancellationToken cancellationToken)
    {
        var invoices = await _db.Invoices.AsNoTracking()
            .Include(i => i.Payments)
            .Where(i => i.Status == InvoiceStatus.Issued || i.Status == InvoiceStatus.Overdue)
            .ToListAsync(cancellationToken);

        return _dunning.Summarize(invoices, today);
    }
}