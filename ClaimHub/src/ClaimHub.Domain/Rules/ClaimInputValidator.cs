using ClaimHub.Domain.Entities;
using ClaimHub.Domain.Enums;
using ClaimHub.Domain.Models;

namespace ClaimHub.Domain.Rules;

public class ClaimInputValidator
{
    public ValidationReport Validate(Claim claim, Policy policy, DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(claim);
        ArgumentNullException.ThrowIfNull(policy);

        var report = new ValidationReport();
        report.AddIf(claim.DateOfLoss > today, nameof(claim.DateOfLoss), "The date of loss cannot lie in the future.");
        report.AddIf(claim.DateOfLoss < policy.StartDate, nameof(claim.DateOfLoss), "The loss happened before the policy started.");
        report.AddIf(claim.PolicyId != policy.Id, nameof(claim.PolicyId), "The claim belongs to a different policy.");

        if (claim.CoverageId is { } coverageId && policy.Coverages.All(c => c.Id != coverageId))
        {
            report.Add(nameof(claim.CoverageId), "The coverage does not belong to this policy.");
        }

        report.Merge(ValidateAmounts(claim));
        return report;
    }

    public ValidationReport ValidateAmounts(Claim claim)
    {
        ArgumentNullException.ThrowIfNull(claim);

        var report = new ValidationReport();
        report.AddIf(claim.AssessedAmount < 0m, nameof(claim.AssessedAmount), "The assessed amount cannot be negative.");
        report.AddIf(claim.ApprovedAmount < 0m, nameof(claim.ApprovedAmount), "The approved amount cannot be negative.");
        report.AddIf(claim.PaidAmount < 0m, nameof(claim.PaidAmount), "The paid amount cannot be negative.");
        report.AddIf(claim.ApprovedAmount is not null && claim.AssessedAmount is null, nameof(claim.ApprovedAmount), "A claim cannot be approved before it is assessed.");
        report.AddIf(claim.PaidAmount > 0m && claim.Status is not (ClaimStatus.PaidOut or ClaimStatus.Closed), nameof(claim.PaidAmount),
            "Only a paid-out or closed claim can carry a paid amount.");
        return report;
    }

    public ValidationReport ValidateDocument(ClaimDocument document, long maxSizeBytes)
    {
        ArgumentNullException.ThrowIfNull(document);

        var report = new ValidationReport();
        report.AddIf(string.IsNullOrWhiteSpace(document.FileName), nameof(document.FileName), "The file name is required.");
        report.AddIf(document.SizeBytes <= 0, nameof(document.SizeBytes), "The file is empty.");
        report.AddIf(document.SizeBytes > maxSizeBytes, nameof(document.SizeBytes), $"The file exceeds {maxSizeBytes} bytes.");
        report.AddIf(string.IsNullOrWhiteSpace(document.Extension), nameof(document.FileName), "The file needs an extension.");
        return report;
    }
}
