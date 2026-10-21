using ClaimHub.Domain.Common;
using ClaimHub.Domain.Entities;
using ClaimHub.Domain.Enums;
using ClaimHub.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ClaimHub.Admin.Pages.Policies;

public class RecalculateModel : PageModel
{
    private const decimal TaxRate = 0.0925m;

    private readonly ClaimHubDbContext _db;

    public RecalculateModel(ClaimHubDbContext db)
    {
        _db = db;
    }

    public Policy Policy { get; private set; } = null!;

    public Recalculation Result { get; private set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        if (!await LoadAsync(id))
        {
            return NotFound();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        if (!await LoadAsync(id))
        {
            return NotFound();
        }

        if (Policy.Status is PolicyStatus.Cancelled or PolicyStatus.Expired or PolicyStatus.Renewed)
        {
            TempData["Error"] = $"A {Policy.Status} policy cannot be recalculated.";
            return RedirectToPage("Details", new { id });
        }

        Policy.AnnualPremium = Result.AnnualPremium;
        Policy.TaxAmount = Result.TaxAmount;
        Policy.DiscountPercent = Math.Round(Result.DiscountRate * 100, 2);
        Policy.UpdatedAt = DateTimeOffset.UtcNow;

        foreach (var coverage in Policy.Coverages.Where(c => c.IsActive))
        {
            if (Result.CoveragePremiums.TryGetValue(coverage.Type, out decimal premium))
            {
                coverage.Premium = premium;
            }
        }

        await _db.SaveChangesAsync();

        TempData["Success"] = $"Premium of {Policy.PolicyNumber} was recalculated to {Result.AnnualPremium:N2}.";
        return RedirectToPage("Details", new { id });
    }

    private async Task<bool> LoadAsync(int id)
    {
        var policy = await _db.Policies
            .Include(p => p.PolicyHolder)
            .Include(p => p.Coverages)
            .Include(p => p.HomeRisk)
            .Include(p => p.MotorRisk)
            .Include(p => p.TravelRisk)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        if (policy == null)
        {
            return false;
        }

        Policy = policy;
        Result = Calculate(policy);
        return true;
    }

    private static Recalculation Calculate(Policy policy)
    {
        var today = BelgianTime.Today();
        var coverageBase = new Dictionary<CoverageType, decimal>();
        var factors = new List<(string Name, decimal Value)>();

        foreach (var coverage in policy.Coverages.Where(c => c.IsActive))
        {
            coverageBase[coverage.Type] = BasePremium(policy, coverage.Type);
        }

        switch (policy.ProductType)
        {
            case ProductType.Home when policy.HomeRisk is { } home:
                factors.Add(("Postcode zone " + home.PostcodeZone, home.PostcodeZone switch { <= 1 => 0.9m, 2 => 1.0m, 3 => 1.15m, _ => 1.4m }));
                factors.Add(("Construction " + home.ConstructionYear, home.ConstructionYear switch { < 1945 => 1.3m, < 1990 => 1.1m, >= 2010 => 0.92m, _ => 1.0m }));
                break;
            case ProductType.Motor when policy.MotorRisk is { } motor:
                int age = today.Year - motor.DriverDateOfBirth.Year - (today < motor.DriverDateOfBirth.AddYears(today.Year - motor.DriverDateOfBirth.Year) ? 1 : 0);
                factors.Add(("Driver age " + age, age switch { < 21 => 1.9m, < 25 => 1.55m, < 30 => 1.2m, >= 70 => 1.25m, _ => 1.0m }));
                factors.Add(("Power " + motor.PowerKw + " kW", motor.PowerKw > 150 ? 1.35m : motor.PowerKw > 100 ? 1.15m : 1.0m));
                break;
            case ProductType.Travel when policy.TravelRisk is { } travel:
                factors.Add(("Scope " + travel.Scope, travel.Scope == TravelScope.Worldwide ? 1.45m : 1.0m));
                break;
        }

        decimal basePremium = coverageBase.Values.Sum();
        decimal riskFactor = factors.Aggregate(1m, (product, factor) => product * factor.Value);

        decimal discountRate = policy.MotorRisk is { } risk ? risk.ClaimFreeYears * 0.05m : 0m;
        var holder = policy.PolicyHolder;
        if (holder != null)
        {
            if (holder.ActivePolicyCount(today) >= 2)
            {
                discountRate += 0.10m;
            }

            discountRate += Math.Min(holder.FullYearsAsCustomer(today) / 5 * 0.02m, 0.06m);
        }

        decimal net = basePremium * riskFactor * (1 - discountRate);
        decimal tax = Math.Round(net * TaxRate, 2);
        decimal annual = Math.Round(net + tax, 2);

        var coveragePremiums = coverageBase.ToDictionary(
            pair => pair.Key,
            pair => basePremium == 0 ? 0m : Math.Round(annual * pair.Value / basePremium, 2));

        int installments = policy.PaymentFrequency switch
        {
            PaymentFrequency.Quarterly => 4,
            PaymentFrequency.Monthly => 12,
            _ => 1
        };

        return new Recalculation(
            Math.Round(basePremium, 2),
            factors,
            riskFactor,
            discountRate,
            Math.Round(net, 2),
            tax,
            annual,
            installments,
            Math.Round(annual / installments, 2),
            coveragePremiums);
    }

    private static decimal BasePremium(Policy policy, CoverageType coverage)
    {
        var home = policy.HomeRisk;
        var motor = policy.MotorRisk;
        return coverage switch
        {
            CoverageType.Building when home != null => 95m + home.BuildingValue * 0.00085m,
            CoverageType.Contents when home != null => 40m + home.ContentsValue * 0.0016m,
            CoverageType.ThirdPartyLiability when motor != null => 285m + motor.PowerKw * 1.9m,
            CoverageType.Comprehensive when motor != null => motor.VehicleValue * 0.021m,
            CoverageType.Glass => 38m,
            CoverageType.Travel => policy.TravelRisk?.PartyType == TravelPartyType.Family ? 98m : 52m,
            _ => 0m
        };
    }

    public record Recalculation(
        decimal BasePremium,
        IReadOnlyList<(string Name, decimal Value)> Factors,
        decimal RiskFactor,
        decimal DiscountRate,
        decimal NetPremium,
        decimal TaxAmount,
        decimal AnnualPremium,
        int InstallmentCount,
        decimal InstallmentAmount,
        IReadOnlyDictionary<CoverageType, decimal> CoveragePremiums);
}
