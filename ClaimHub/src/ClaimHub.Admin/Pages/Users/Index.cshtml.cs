using ClaimHub.Domain.Entities;
using ClaimHub.Domain.Enums;
using ClaimHub.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ClaimHub.Admin.Pages.Users;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ClaimHubDbContext _db;

    public IndexModel(ClaimHubDbContext db)
    {
        _db = db;
    }

    public IReadOnlyList<ApplicationUser> Users { get; private set; } = Array.Empty<ApplicationUser>();

    public async Task<IActionResult> OnGetAsync()
    {
        if (!User.IsInRole("Admin"))
        {
            return Forbid();
        }

        Users = await _db.Users
            .OrderBy(u => u.Role)
            .ThenBy(u => u.Email)
            .ToListAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostChangeRoleAsync(int id, UserRole role)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
        {
            return NotFound();
        }

        user.Role = role;
        await _db.SaveChangesAsync();

        TempData["Success"] = $"{user.DisplayName} is now {role}.";
        return RedirectToPage();
    }
}
