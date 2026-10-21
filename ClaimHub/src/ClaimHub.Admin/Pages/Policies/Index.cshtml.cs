using ClaimHub.Admin.Infrastructure;
using ClaimHub.Domain.Entities;
using ClaimHub.Domain.Enums;
using ClaimHub.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ClaimHub.Admin.Pages.Policies;

public class IndexModel : PageModel
{
    private readonly ClaimHubDbContext _db;

    public IndexModel(ClaimHubDbContext db)
    {
        _db = db;
    }

    [BindProperty(SupportsGet = true)]
    public string? Q { get; set; }

    [BindProperty(SupportsGet = true)]
    public PolicyStatus? Status { get; set; }

    [BindProperty(SupportsGet = true)]
    public ProductType? Product { get; set; }

    [BindProperty(SupportsGet = true, Name = "p")]
    public int PageNumber { get; set; } = 1;

    public PageInfo Pager { get; private set; } = new(1, 0);

    public IReadOnlyList<Policy> Policies { get; private set; } = Array.Empty<Policy>();

    public async Task OnGetAsync()
    {
        var query = _db.Policies.Where(p => !p.IsDeleted);

        if (Status.HasValue)
        {
            query = query.Where(p => p.Status == Status.Value);
        }

        if (Product.HasValue)
        {
            query = query.Where(p => p.ProductType == Product.Value);
        }

        if (!string.IsNullOrWhiteSpace(Q))
        {
            string term = Q.Trim();
            query = query.Where(p => p.PolicyNumber.Contains(term) || p.PolicyHolder!.LastName.Contains(term));
        }

        Pager = new PageInfo(PageNumber, await query.CountAsync());

        Policies = await query
            .Include(p => p.PolicyHolder)
            .OrderByDescending(p => p.StartDate)
            .ThenBy(p => p.PolicyNumber)
            .Skip(Pager.Skip)
            .Take(Pager.PageSize)
            .ToListAsync();
    }
}
