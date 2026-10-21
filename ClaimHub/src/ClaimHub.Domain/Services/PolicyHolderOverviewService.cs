using ClaimHub.Domain.Entities;
using ClaimHub.Domain.Enums;
using ClaimHub.Domain.Models;

namespace ClaimHub.Domain.Services;

/// <summary>
/// Builds the one-page view of a policyholder: policies, claims, open invoices and customer standing.
/// </summary>
public class PolicyHolderOverviewService
{
    public const int EstablishedYears = 2;
    public const int LoyalYears = 5;

    public PolicyHolderOverview Build(PolicyHolder holder, DateOnly asOf)
    {
        ArgumentNullException.ThrowIfNull(holder);

        var policies = holder.Policies
            .Where(p => !p.IsDeleted)
            .OrderBy(p => p.StartDate)
            .ThenBy(p => p.Id)
            .ToList();

        var lines = policies.Select(ToLine).ToList();
        var inForce = policies.Where(p => p.IsInForceOn(asOf)).ToList();
        int years = holder.FullYearsAsCustomer(asOf);

        return new PolicyHolderOverview
        {
            PolicyHolderId = holder.Id,
            FullName = holder.FullName,
            AsOf = asOf,
            YearsAsCustomer = years,
            Tier = DetermineTier(holder, years),
            Policies = lines,
            InForceCount = inForce.Count,
            OpenClaimCount = policies.SelectMany(p => p.Claims).Count(IsOpen),
            CreditLimit = holder.CreditLimit,
            NextExpiry = inForce.Count == 0 ? null : inForce.Min(p => p.EndDate),
            Notes = BuildNotes(holder, lines, asOf)
        };
    }

    public CustomerTier DetermineTier(PolicyHolder holder, int yearsAsCustomer)
    {
        if (holder.IsVip)
        {
            return CustomerTier.Vip;
        }

        return yearsAsCustomer switch
        {
            >= LoyalYears => CustomerTier.Loyal,
            >= EstablishedYears => CustomerTier.Established,
            _ => CustomerTier.New
        };
    }

    public HolderPolicyLine ToLine(Policy policy)
    {
        var claims = policy.Claims.Where(c => !c.IsDeleted).ToList();
        var openInvoices = policy.Invoices
            .Where(i => i.Status is InvoiceStatus.Issued or InvoiceStatus.Overdue)
            .Sum(i => i.OutstandingAmount);

        return new HolderPolicyLine(
            policy.Id,
            policy.PolicyNumber,
            policy.ProductType,
            policy.Status,
            policy.StartDate,
            policy.EndDate,
            policy.AnnualPremium,
            claims.Count,
            claims.Sum(c => c.PaidAmount),
            openInvoices);
    }

    private static bool IsOpen(Claim claim) =>
        !claim.IsDeleted && claim.Status is ClaimStatus.Reported or ClaimStatus.UnderAssessment;

    private static IReadOnlyList<string> BuildNotes(PolicyHolder holder, IReadOnlyList<HolderPolicyLine> lines, DateOnly asOf)
    {
        var notes = new List<string>();

        decimal outstanding = lines.Sum(l => l.OutstandingInvoices);
        if (holder.CreditLimit > 0m && outstanding > holder.CreditLimit)
        {
            notes.Add($"Open invoices of {outstanding:N2} exceed the credit limit of {holder.CreditLimit:N2}.");
        }

        foreach (var line in lines.Where(l => l.Status == PolicyStatus.Active && l.LossRatio >= 1m))
        {
            notes.Add($"Paid claims on {line.PolicyNumber} are at least equal to its annual premium.");
        }

        foreach (var line in lines.Where(l => l.Status == PolicyStatus.Active && l.EndDate >= asOf && l.EndDate.DayNumber - asOf.DayNumber <= 30))
        {
            notes.Add($"{line.PolicyNumber} ends on {line.EndDate:yyyy-MM-dd}.");
        }

        if (lines.Count(l => l.Status == PolicyStatus.Suspended) > 0)
        {
            notes.Add("At least one policy is suspended.");
        }

        return notes;
    }
}