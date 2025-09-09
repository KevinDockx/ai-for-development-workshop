using ClaimHub.Domain.Common;
using ClaimHub.Domain.Entities;
using ClaimHub.Domain.Enums;
using ClaimHub.Domain.Models;

namespace ClaimHub.Domain.Services;

/// <summary>
/// Wraps the cancellation and endorsement calculators and adds the context a decision maker needs next to the numbers.
/// </summary>
public class PolicyChangeDecisionService
{
    private readonly CancellationRefundCalculator _refunds;
    private readonly EndorsementPremiumCalculator _endorsements;

    public PolicyChangeDecisionService()
        : this(new CancellationRefundCalculator(), new EndorsementPremiumCalculator())
    {
    }

    public PolicyChangeDecisionService(CancellationRefundCalculator refunds, EndorsementPremiumCalculator endorsements)
    {
        _refunds = refunds;
        _endorsements = endorsements;
    }

    public CancellationDecisionSummary DescribeCancellation(Policy policy, DateOnly effectiveOn, IEnumerable<Invoice>? invoices = null)
    {
        ArgumentNullException.ThrowIfNull(policy);

        var refund = _refunds.Calculate(policy, effectiveOn);
        var term = new PolicyTerm(policy.StartDate, policy.EndDate);

        var paidClaims = policy.Claims
            .Where(c => !c.IsDeleted && c.PaidAmount > 0m && term.Contains(c.DateOfLoss))
            .ToList();

        var openInvoices = (invoices ?? policy.Invoices)
            .Where(i => i.PolicyId == policy.Id && IsOpenAndNotYetDue(i, refund.EffectiveDate))
            .OrderBy(i => i.DueDate)
            .ThenBy(i => i.Id)
            .ToList();

        return new CancellationDecisionSummary
        {
            PolicyId = policy.Id,
            PolicyNumber = policy.PolicyNumber,
            Refund = refund,
            PaidClaimCount = paidClaims.Count,
            PaidClaimsInTerm = paidClaims.Sum(c => c.PaidAmount),
            InvoiceIdsToCancel = openInvoices.Select(i => i.Id).ToList(),
            OutstandingOnOpenInvoices = openInvoices.Sum(i => i.OutstandingAmount)
        };
    }

    public EndorsementDecisionSummary DescribeEndorsement(
        Policy policy,
        decimal newAnnualPremium,
        DateOnly effectiveDate,
        EndorsementType type = EndorsementType.ValueAdjustment)
    {
        ArgumentNullException.ThrowIfNull(policy);

        var premium = _endorsements.Calculate(policy, newAnnualPremium, effectiveDate);

        var direction = premium.AdditionalPremium switch
        {
            > 0m => PremiumDirection.Additional,
            < 0m => PremiumDirection.Credit,
            _ => PremiumDirection.NoChange
        };

        return new EndorsementDecisionSummary
        {
            PolicyId = policy.Id,
            PolicyNumber = policy.PolicyNumber,
            Type = type,
            Premium = premium,
            Direction = direction,
            RemainingShare = Math.Round(premium.RemainingDays / (decimal)premium.TermDays, 4, MoneyMath.Rounding)
        };
    }

    public decimal TotalEndorsementDelta(Policy policy) =>
        policy.Endorsements.Sum(e => e.PremiumDelta);

    public IReadOnlyList<PolicyEndorsement> EndorsementsInTerm(Policy policy) =>
        policy.Endorsements
            .Where(e => e.EffectiveDate >= policy.StartDate && e.EffectiveDate < policy.EndDate)
            .OrderBy(e => e.EffectiveDate)
            .ThenBy(e => e.Id)
            .ToList();

    private static bool IsOpenAndNotYetDue(Invoice invoice, DateOnly effectiveOn) =>
        invoice.Status is InvoiceStatus.Issued or InvoiceStatus.Overdue && invoice.DueDate > effectiveOn;
}