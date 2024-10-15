using System.ComponentModel.DataAnnotations;
using ClaimHub.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ClaimHub.Admin.Pages.PolicyHolders;

public class EditModel : PageModel
{
    private readonly ClaimHubDbContext _db;

    public EditModel(ClaimHubDbContext db)
    {
        _db = db;
    }

    [BindProperty]
    public HolderInput Input { get; set; } = new();

    public int HolderId { get; private set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var holder = await _db.PolicyHolders.FirstOrDefaultAsync(h => h.Id == id && !h.IsDeleted);
        if (holder == null)
        {
            return NotFound();
        }

        HolderId = id;
        Input = new HolderInput
        {
            FirstName = holder.FirstName,
            LastName = holder.LastName,
            Email = holder.Email,
            Phone = holder.Phone,
            Street = holder.Street,
            PostalCode = holder.PostalCode,
            City = holder.City,
            IsVip = holder.IsVip
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var holder = await _db.PolicyHolders.FirstOrDefaultAsync(h => h.Id == id && !h.IsDeleted);
        if (holder == null)
        {
            return NotFound();
        }

        HolderId = id;
        if (!ModelState.IsValid)
        {
            return Page();
        }

        holder.FirstName = Input.FirstName.Trim();
        holder.LastName = Input.LastName.Trim();
        holder.Email = Input.Email.Trim();
        holder.Phone = string.IsNullOrWhiteSpace(Input.Phone) ? null : Input.Phone.Trim();
        holder.Street = Input.Street.Trim();
        holder.PostalCode = Input.PostalCode.Trim();
        holder.City = Input.City.Trim();
        holder.IsVip = Input.IsVip;
        holder.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync();

        TempData["Success"] = $"{holder.FullName} was updated.";
        return RedirectToPage("Details", new { id });
    }

    public class HolderInput
    {
        [Required, StringLength(80)]
        [Display(Name = "First name")]
        public string FirstName { get; set; } = string.Empty;

        [Required, StringLength(80)]
        [Display(Name = "Last name")]
        public string LastName { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(200)]
        public string Email { get; set; } = string.Empty;

        [Phone, StringLength(40)]
        public string? Phone { get; set; }

        [Required, StringLength(120)]
        public string Street { get; set; } = string.Empty;

        [Required, StringLength(10)]
        [Display(Name = "Postal code")]
        public string PostalCode { get; set; } = string.Empty;

        [Required, StringLength(80)]
        public string City { get; set; } = string.Empty;

        [Display(Name = "VIP customer")]
        public bool IsVip { get; set; }
    }
}
