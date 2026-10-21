using ClaimHub.Domain.Entities;
using ClaimHub.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ClaimHub.Admin.Pages.Claims;

public class DeleteModel : PageModel
{
    private readonly ClaimHubDbContext _db;

    public DeleteModel(ClaimHubDbContext db)
    {
        _db = db;
    }

    public Claim Claim { get; private set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var claim = await _db.Claims.FirstOrDefaultAsync(c => c.Id == id);
        if (claim == null)
        {
            return NotFound();
        }

        Claim = claim;
        return Page();
    }

    // TODO(pdewitte, 2024-09-02): switch to soft delete once audit trail lands (CLM-0176).
    public async Task<IActionResult> OnPostAsync(int id)
    {
        var claim = await _db.Claims.FirstOrDefaultAsync(c => c.Id == id);
        if (claim == null)
        {
            return NotFound();
        }

        var payouts = await _db.Payments.Where(p => p.ClaimId == id).ToListAsync();
        _db.Payments.RemoveRange(payouts);
        _db.Claims.Remove(claim);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Claim {claim.ClaimNumber} was deleted.";
        return RedirectToPage("Index");
    }
}
