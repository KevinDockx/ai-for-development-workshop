using System.ComponentModel.DataAnnotations;
using ClaimHub.Domain.Entities;
using ClaimHub.Domain.Enums;
using ClaimHub.Domain.Models;
using ClaimHub.Domain.Rules;
using ClaimHub.Domain.Services;
using ClaimHub.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ClaimHub.Admin.Pages.Claims;

public class DetailsModel : PageModel
{
    private static readonly ClaimStateMachine StateMachine = new();

    private readonly ClaimHubDbContext _db;
    private readonly ClaimDecisionService _decisions;

    public DetailsModel(ClaimHubDbContext db, ClaimDecisionService decisions)
    {
        _db = db;
        _decisions = decisions;
    }

    public Claim Claim { get; private set; } = null!;

    public ClaimDecisionSummary? Decision { get; private set; }

    public bool CanAssess => User.IsInRole("Admin") || User.IsInRole("ClaimsHandler") || User.IsInRole("SeniorClaimsHandler");

    [BindProperty]
    [Range(0.01, 1_000_000)]
    [Display(Name = "Amount")]
    public decimal? Amount { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        return await LoadAsync(id) ? Page() : NotFound();
    }

    public async Task<IActionResult> OnPostStartAssessmentAsync(int id)
    {
        if (!CanAssess)
        {
            return Forbid();
        }

        if (!await LoadAsync(id))
        {
            return NotFound();
        }

        return await TransitionAsync(ClaimStatus.UnderAssessment, "Assessment started.");
    }

    public async Task<IActionResult> OnPostAssessAsync(int id)
    {
        if (!CanAssess)
        {
            return Forbid();
        }

        if (!await LoadAsync(id))
        {
            return NotFound();
        }

        if (!ModelState.IsValid || Amount is null)
        {
            return Page();
        }

        if (Claim.Status != ClaimStatus.UnderAssessment)
        {
            TempData["Error"] = "Only a claim under assessment can be assessed.";
            return RedirectToPage(new { id });
        }

        Claim.AssessedAmount = Math.Round(Amount.Value, 2);
        Claim.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Assessed amount of {Claim.ClaimNumber} set to {Claim.AssessedAmount:N2}.";
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostApproveAsync(int id)
    {
        if (!CanAssess)
        {
            return Forbid();
        }

        if (!await LoadAsync(id))
        {
            return NotFound();
        }

        if (!ModelState.IsValid || Amount is null)
        {
            return Page();
        }

        if (Claim.AssessedAmount is null)
        {
            TempData["Error"] = "Assess the claim before approving it.";
            return RedirectToPage(new { id });
        }

        if (Amount > Claim.AssessedAmount)
        {
            ModelState.AddModelError(nameof(Amount), "The approved amount cannot exceed the assessed amount.");
            return Page();
        }

        Claim.ApprovedAmount = Math.Round(Amount.Value, 2);
        return await TransitionAsync(ClaimStatus.Approved, $"Approved for {Claim.ApprovedAmount:N2}.");
    }

    private async Task<bool> LoadAsync(int id)
    {
        var claim = await _db.Claims
            .Include(c => c.Policy).ThenInclude(p => p!.PolicyHolder)
            .Include(c => c.Coverage)
            .Include(c => c.AssignedHandler)
            .Include(c => c.StatusHistory)
            .Include(c => c.FraudFlags)
            .Include(c => c.Documents)
            .AsSplitQuery()
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        if (claim == null)
        {
            return false;
        }

        Claim = claim;
        await BuildDecisionAsync(claim);
        return true;
    }

    private async Task BuildDecisionAsync(Claim claim)
    {
        if (claim.Policy == null)
        {
            return;
        }

        var holderClaims = await _db.Claims
            .Where(c => c.Policy!.PolicyHolderId == claim.Policy.PolicyHolderId && !c.IsDeleted)
            .ToListAsync();
        Decision = _decisions.Summarize(claim, claim.Policy, holderClaims, DateTimeOffset.UtcNow);
    }

    private async Task<IActionResult> TransitionAsync(ClaimStatus target, string note)
    {
        var result = StateMachine.Apply(Claim, target, User.Identity?.Name ?? "unknown", DateTimeOffset.UtcNow, note);
        if (!result.IsAllowed)
        {
            TempData["Error"] = result.Reason;
            return RedirectToPage(new { id = Claim.Id });
        }

        await _db.SaveChangesAsync();
        TempData["Success"] = $"{Claim.ClaimNumber} is now {Claim.Status}.";
        return RedirectToPage(new { id = Claim.Id });
    }
}
