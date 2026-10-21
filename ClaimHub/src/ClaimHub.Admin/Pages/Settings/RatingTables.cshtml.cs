using System.ComponentModel.DataAnnotations;
using ClaimHub.Domain.Entities;
using ClaimHub.Domain.Enums;
using ClaimHub.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ClaimHub.Admin.Pages.Settings;

public class RatingTablesModel : PageModel
{
    private readonly ClaimHubDbContext _db;

    public RatingTablesModel(ClaimHubDbContext db)
    {
        _db = db;
    }

    [BindProperty(SupportsGet = true)]
    public ProductType? Product { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? EditId { get; set; }

    [BindProperty]
    public RatingInput Input { get; set; } = new();

    public IReadOnlyList<RatingTableEntry> Entries { get; private set; } = Array.Empty<RatingTableEntry>();

    public async Task OnGetAsync()
    {
        await LoadAsync();

        var editing = Entries.FirstOrDefault(e => e.Id == EditId);
        if (editing != null)
        {
            Input = new RatingInput { Multiplier = editing.Multiplier, ValidFrom = editing.ValidFrom, ValidTo = editing.ValidTo };
        }
    }

    public async Task<IActionResult> OnPostSaveAsync(int id)
    {
        var entry = await _db.RatingTable.FirstOrDefaultAsync(e => e.Id == id);
        if (entry == null)
        {
            return NotFound();
        }

        if (Input.ValidTo < Input.ValidFrom)
        {
            ModelState.AddModelError("Input.ValidTo", "The end date cannot be before the start date.");
        }

        if (!ModelState.IsValid)
        {
            EditId = id;
            await LoadAsync();
            return Page();
        }

        entry.Multiplier = Input.Multiplier;
        entry.ValidFrom = Input.ValidFrom;
        entry.ValidTo = Input.ValidTo;
        await _db.SaveChangesAsync();

        TempData["Success"] = $"{entry.Factor} / {entry.Key} was updated.";
        return RedirectToPage(new { product = Product });
    }

    private async Task LoadAsync()
    {
        var query = _db.RatingTable.AsQueryable();
        if (Product.HasValue)
        {
            query = query.Where(e => e.ProductType == Product.Value);
        }

        Entries = await query
            .OrderBy(e => e.ProductType)
            .ThenBy(e => e.Factor)
            .ThenBy(e => e.Key)
            .ThenBy(e => e.ValidFrom)
            .ToListAsync();
    }

    public class RatingInput
    {
        [Required, Range(0.1, 5.0)]
        public decimal Multiplier { get; set; } = 1m;

        [Required]
        [Display(Name = "Valid from")]
        public DateOnly ValidFrom { get; set; }

        [Display(Name = "Valid to")]
        public DateOnly? ValidTo { get; set; }
    }
}
