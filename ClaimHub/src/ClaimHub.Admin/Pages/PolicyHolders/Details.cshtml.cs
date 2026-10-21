using ClaimHub.Domain.Common;
using ClaimHub.Domain.Entities;
using ClaimHub.Domain.Models;
using ClaimHub.Domain.Services;
using ClaimHub.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ClaimHub.Admin.Pages.PolicyHolders;

public class DetailsModel : PageModel
{
    private readonly ClaimHubDbContext _db;
    private readonly PolicyHolderOverviewService _overviews;
    private readonly AccountStatementService _statements;

    public DetailsModel(ClaimHubDbContext db, PolicyHolderOverviewService overviews, AccountStatementService statements)
    {
        _db = db;
        _overviews = overviews;
        _statements = statements;
    }

    public AccountStatement Statement { get; private set; } = null!;

    public PolicyHolder Holder { get; private set; } = null!;

    public PolicyHolderOverview Overview { get; private set; } = null!;

    public int ClaimCount { get; private set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var holder = await _db.PolicyHolders
            .Include(h => h.Broker)
            .Include(h => h.Policies).ThenInclude(p => p.Claims)
            .Include(h => h.Policies).ThenInclude(p => p.Invoices).ThenInclude(i => i.Payments)
            .AsSplitQuery()
            .FirstOrDefaultAsync(h => h.Id == id && !h.IsDeleted);
        if (holder == null)
        {
            return NotFound();
        }

        Holder = holder;
        Overview = _overviews.Build(holder, BelgianTime.Today());
        Statement = _statements.Build(holder.Id, holder.Policies);
        ClaimCount = await _db.Claims.CountAsync(c => !c.IsDeleted && c.Policy!.PolicyHolderId == id);
        return Page();
    }
}
