using ClaimHub.Admin.Infrastructure;
using ClaimHub.Domain.Common;
using ClaimHub.Domain.Entities;
using ClaimHub.Domain.Enums;
using ClaimHub.Domain.Models;
using ClaimHub.Infrastructure.Data;
using ClaimHub.Infrastructure.Reporting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ClaimHub.Admin.Pages.Claims;

public class IndexModel : PageModel
{
    private readonly ClaimHubDbContext _db;
    private readonly HandlerWorkloadQuery _workload;

    public IndexModel(ClaimHubDbContext db, HandlerWorkloadQuery workload)
    {
        _db = db;
        _workload = workload;
    }

    public HandlerWorkloadReport Workload { get; private set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? Q { get; set; }

    [BindProperty(SupportsGet = true)]
    public ClaimStatus? Status { get; set; }

    [BindProperty(SupportsGet = true, Name = "p")]
    public int PageNumber { get; set; } = 1;

    public PageInfo Pager { get; private set; } = new(1, 0);

    public IReadOnlyList<Claim> Claims { get; private set; } = Array.Empty<Claim>();

    public async Task OnGetAsync()
    {
        Workload = await _workload.GetReportAsync(BelgianTime.Today(), HttpContext.RequestAborted);

        var query = _db.Claims.Where(c => !c.IsDeleted);

        if (Status.HasValue)
        {
            query = query.Where(c => c.Status == Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(Q))
        {
            string term = Q.Trim();
            query = query.Where(c => c.ClaimNumber.Contains(term));
        }

        Pager = new PageInfo(PageNumber, await query.CountAsync());

        Claims = await query
            .OrderByDescending(c => c.ReportedAt)
            .Skip(Pager.Skip)
            .Take(Pager.PageSize)
            .ToListAsync();
    }
}
