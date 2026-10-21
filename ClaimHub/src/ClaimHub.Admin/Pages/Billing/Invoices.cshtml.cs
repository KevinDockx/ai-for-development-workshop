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

namespace ClaimHub.Admin.Pages.Billing;

public class InvoicesModel : PageModel
{
    private readonly ClaimHubDbContext _db;
    private readonly ReceivablesQuery _receivables;

    public InvoicesModel(ClaimHubDbContext db, ReceivablesQuery receivables)
    {
        _db = db;
        _receivables = receivables;
    }

    public ReceivablesAging Aging { get; private set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? Q { get; set; }

    [BindProperty(SupportsGet = true)]
    public InvoiceStatus? Status { get; set; }

    [BindProperty(SupportsGet = true, Name = "p")]
    public int PageNumber { get; set; } = 1;

    public PageInfo Pager { get; private set; } = new(1, 0);

    public IReadOnlyList<Invoice> Invoices { get; private set; } = Array.Empty<Invoice>();

    public async Task OnGetAsync()
    {
        Aging = await _receivables.GetAgingAsync(BelgianTime.Today(), HttpContext.RequestAborted);

        var query = _db.Invoices.AsQueryable();

        if (Status.HasValue)
        {
            query = query.Where(i => i.Status == Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(Q))
        {
            string term = Q.Trim();
            query = query.Where(i => i.InvoiceNumber.Contains(term) || i.Policy!.PolicyNumber.Contains(term));
        }

        Pager = new PageInfo(PageNumber, await query.CountAsync());

        Invoices = await query
            .Include(i => i.Policy)
            .Include(i => i.Payments)
            .OrderByDescending(i => i.IssuedOn)
            .ThenByDescending(i => i.Id)
            .Skip(Pager.Skip)
            .Take(Pager.PageSize)
            .ToListAsync();
    }
}
