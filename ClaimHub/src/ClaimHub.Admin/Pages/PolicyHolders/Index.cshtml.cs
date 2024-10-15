using ClaimHub.Admin.Infrastructure;
using ClaimHub.Domain.Entities;
using ClaimHub.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ClaimHub.Admin.Pages.PolicyHolders;

public class IndexModel : PageModel
{
    private readonly ClaimHubDbContext _db;

    public IndexModel(ClaimHubDbContext db)
    {
        _db = db;
    }

    [BindProperty(SupportsGet = true)]
    public string? Q { get; set; }

    [BindProperty(SupportsGet = true, Name = "p")]
    public int PageNumber { get; set; } = 1;

    public PageInfo Pager { get; private set; } = new(1, 0);

    public IReadOnlyList<HolderRow> Holders { get; private set; } = Array.Empty<HolderRow>();

    public async Task OnGetAsync()
    {
        var query = _db.PolicyHolders.Where(h => !h.IsDeleted);

        if (!string.IsNullOrWhiteSpace(Q))
        {
            string term = Q.Trim();
            query = query.Where(h => h.LastName.Contains(term)
                || h.FirstName.Contains(term)
                || h.Email.Contains(term)
                || h.City.Contains(term));
        }

        Pager = new PageInfo(PageNumber, await query.CountAsync());

        Holders = await query
            .OrderBy(h => h.LastName)
            .ThenBy(h => h.FirstName)
            .Skip(Pager.Skip)
            .Take(Pager.PageSize)
            .Select(h => new HolderRow(h.Id, h.FirstName + " " + h.LastName, h.Email, h.City, h.IsVip, h.Policies.Count(p => !p.IsDeleted)))
            .ToListAsync();
    }

    public record HolderRow(int Id, string Name, string Email, string City, bool IsVip, int PolicyCount);
}
