using ClaimHub.Domain.Common;
using ClaimHub.Domain.Entities;
using ClaimHub.Domain.Enums;
using ClaimHub.Domain.Models;
using ClaimHub.Infrastructure.Data;
using ClaimHub.Infrastructure.Reporting;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ClaimHub.Admin.Pages;

public class DashboardModel : PageModel
{
    private readonly ClaimHubDbContext _db;
    private readonly PortfolioReadService _portfolio;
    private readonly RenewalPipelineQuery _renewals;

    public DashboardModel(ClaimHubDbContext db, PortfolioReadService portfolio, RenewalPipelineQuery renewals)
    {
        _db = db;
        _portfolio = portfolio;
        _renewals = renewals;
    }

    public IReadOnlyList<RenewalPipelineItem> Renewals { get; private set; } = Array.Empty<RenewalPipelineItem>();

    public PolicyPortfolioSummary Portfolio { get; private set; } = new();

    public ClaimQueueSummary ClaimQueue { get; private set; } = new();

    public int PolicyHolderCount { get; private set; }

    public int ActivePolicyCount { get; private set; }

    public int OpenClaimCount { get; private set; }

    public int OverdueInvoiceCount { get; private set; }

    public decimal OverdueAmount { get; private set; }

    public IReadOnlyList<Claim> RecentClaims { get; private set; } = Array.Empty<Claim>();

    public IReadOnlyList<Invoice> RecentInvoices { get; private set; } = Array.Empty<Invoice>();

    public async Task OnGetAsync()
    {
        var today = BelgianTime.Today();

        PolicyHolderCount = await _db.PolicyHolders.CountAsync(h => !h.IsDeleted);
        ActivePolicyCount = await _db.Policies.CountAsync(p => !p.IsDeleted && p.Status == PolicyStatus.Active);
        OpenClaimCount = await _db.Claims.CountAsync(c => !c.IsDeleted
            && (c.Status == ClaimStatus.Reported || c.Status == ClaimStatus.UnderAssessment));

        var overdue = await _db.Invoices
            .Include(i => i.Payments)
            .Where(i => (i.Status == InvoiceStatus.Issued || i.Status == InvoiceStatus.Overdue) && i.DueDate < today)
            .ToListAsync();
        OverdueInvoiceCount = overdue.Count;
        OverdueAmount = overdue.Sum(i => i.OutstandingAmount);

        Portfolio = await _portfolio.GetPortfolioAsync(today, HttpContext.RequestAborted);
        ClaimQueue = await _portfolio.GetClaimQueueAsync(DateTimeOffset.UtcNow, HttpContext.RequestAborted);
        Renewals = await _renewals.GetUpcomingAsync(today, HttpContext.RequestAborted);

        RecentClaims = await _db.Claims
            .Include(c => c.Policy)
            .Where(c => !c.IsDeleted)
            .OrderByDescending(c => c.ReportedAt)
            .Take(6)
            .ToListAsync();

        RecentInvoices = await _db.Invoices
            .Include(i => i.Policy)
            .OrderByDescending(i => i.IssuedOn)
            .ThenByDescending(i => i.Id)
            .Take(6)
            .ToListAsync();
    }
}
