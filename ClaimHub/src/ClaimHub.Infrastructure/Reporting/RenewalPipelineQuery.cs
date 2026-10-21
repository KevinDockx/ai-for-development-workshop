using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClaimHub.Domain.Enums;
using ClaimHub.Domain.Models;
using ClaimHub.Domain.Services;
using ClaimHub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ClaimHub.Infrastructure.Reporting;

public sealed record RenewalPipelineItem(
    int PolicyId,
    string PolicyNumber,
    string HolderName,
    ProductType ProductType,
    decimal AnnualPremium,
    RenewalWindow Window,
    bool AlreadyRenewed);

/// <summary>Lists the active policies whose renewal window is open or about to open, with their renewal status.</summary>
public class RenewalPipelineQuery
{
    public const int DefaultLookaheadDays = 30;

    private readonly ClaimHubDbContext _db;
    private readonly PolicyPortfolioService _portfolio;

    public RenewalPipelineQuery(ClaimHubDbContext db, PolicyPortfolioService portfolio)
    {
        _db = db;
        _portfolio = portfolio;
    }

    public async Task<IReadOnlyList<RenewalPipelineItem>> GetUpcomingAsync(
        DateOnly asOf,
        CancellationToken cancellationToken,
        int lookaheadDays = DefaultLookaheadDays)
    {
        var horizon = asOf.AddDays(PolicyPortfolioService.RenewalLeadDays + lookaheadDays);

        var policies = await _db.Policies.AsNoTracking()
            .Include(p => p.PolicyHolder)
            .Where(p => !p.IsDeleted
                && p.Status == PolicyStatus.Active
                && p.EndDate >= asOf
                && p.EndDate <= horizon)
            .OrderBy(p => p.EndDate)
            .ThenBy(p => p.Id)
            .ToListAsync(cancellationToken);

        var renewedIds = await _db.Policies.AsNoTracking()
            .Where(p => p.RenewedFromPolicyId != null)
            .Select(p => p.RenewedFromPolicyId.Value)
            .ToListAsync(cancellationToken);

        return policies
            .Select(p => new RenewalPipelineItem(
                p.Id,
                p.PolicyNumber,
                p.PolicyHolder == null ? string.Empty : p.PolicyHolder.FullName,
                p.ProductType,
                p.AnnualPremium,
                _portfolio.GetRenewalWindow(p, asOf),
                renewedIds.Contains(p.Id)))
            .ToList();
    }
}