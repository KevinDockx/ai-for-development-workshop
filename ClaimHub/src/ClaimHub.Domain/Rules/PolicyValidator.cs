using ClaimHub.Domain.Entities;
using ClaimHub.Domain.Enums;
using ClaimHub.Domain.Models;

namespace ClaimHub.Domain.Rules;

public class PolicyValidator
{
    private readonly RiskValidator _risks;

    public PolicyValidator()
        : this(new RiskValidator())
    {
    }

    public PolicyValidator(RiskValidator risks)
    {
        _risks = risks;
    }

    public ValidationReport Validate(Policy policy, DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(policy);

        var report = new ValidationReport();
        report.AddIf(!Enum.IsDefined(policy.ProductType), nameof(policy.ProductType), "The product type is unknown.");
        report.AddIf(policy.PolicyHolderId <= 0, nameof(policy.PolicyHolderId), "A policy needs a policyholder.");
        report.AddIf(policy.EndDate <= policy.StartDate, nameof(policy.EndDate), "The end date must lie after the start date.");
        report.AddIf(policy.TaxAmount < 0m, nameof(policy.TaxAmount), "The tax amount cannot be negative.");
        report.AddIf(!Enum.IsDefined(policy.PaymentFrequency), nameof(policy.PaymentFrequency), "The payment frequency is unknown.");

        if (policy.Status == PolicyStatus.Cancelled)
        {
            report.AddIf(policy.CancelledOn is null, nameof(policy.CancelledOn), "A cancelled policy needs a cancellation date.");
            report.AddIf(string.IsNullOrWhiteSpace(policy.CancellationReason), nameof(policy.CancellationReason), "A cancelled policy needs a reason.");
        }
        else
        {
            report.AddIf(policy.CancelledOn is not null, nameof(policy.CancelledOn), "Only a cancelled policy has a cancellation date.");
        }

        if (policy.Status == PolicyStatus.Active)
        {
            report.AddIf(policy.EndDate <= today, nameof(policy.Status), "A policy past its end date cannot be active.");
            report.AddIf(policy.Coverages.All(c => !c.IsActive), nameof(policy.Coverages), "An active policy needs at least one active coverage.");
        }

        report.AddIf(policy.RenewedFromPolicyId == policy.Id && policy.Id != 0, nameof(policy.RenewedFromPolicyId), "A policy cannot renew itself.");
        report.Merge(ProductCoverageRules.Validate(policy));
        report.Merge(_risks.ValidateFor(policy, today));
        return report;
    }

    public bool IsReadyForActivation(Policy policy, DateOnly today) =>
        policy.Status == PolicyStatus.Quoted && Validate(policy, today).IsValid;
}
