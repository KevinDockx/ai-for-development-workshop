using ClaimHub.Domain.Common;
using ClaimHub.Domain.Entities;
using ClaimHub.Domain.Models;
using ClaimHub.Domain.Services;
using ClaimHub.Infrastructure.Data;
using ClaimHub.Infrastructure.Reporting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ClaimHub.Admin.Pages.Policies;

public class DetailsModel : PageModel
{
    private readonly ClaimHubDbContext _db;
    private readonly PolicyPortfolioService _portfolio;
    private readonly AuditTrailQuery _audit;
    private readonly RenewalOutlookService _outlooks;

    public DetailsModel(ClaimHubDbContext db, PolicyPortfolioService portfolio, AuditTrailQuery audit, RenewalOutlookService outlooks)
    {
        _db = db;
        _portfolio = portfolio;
        _audit = audit;
        _outlooks = outlooks;
    }

    public RenewalOutlook Outlook { get; private set; } = null!;

    public Policy Policy { get; private set; } = null!;

    public PolicyCoverageSummary CoverageSummary { get; private set; } = null!;

    public IReadOnlyList<AuditTrailItem> AuditTrail { get; private set; } = Array.Empty<AuditTrailItem>();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var policy = await _db.Policies
            .Include(p => p.PolicyHolder)
            .Include(p => p.Broker)
            .Include(p => p.Coverages)
            .Include(p => p.HomeRisk)
            .Include(p => p.MotorRisk)
            .Include(p => p.TravelRisk)
            .Include(p => p.Invoices)
            .Include(p => p.Claims)
            .Include(p => p.Endorsements)
            .AsSplitQuery()
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        if (policy == null)
        {
            return NotFound();
        }

        Policy = policy;
        CoverageSummary = _portfolio.SummarizeCoverage(policy);
        Outlook = _outlooks.Evaluate(policy, BelgianTime.Today());
        AuditTrail = await _audit.ForEntityAsync(nameof(Policy), policy.Id, HttpContext.RequestAborted, 10);
        return Page();
    }
}
