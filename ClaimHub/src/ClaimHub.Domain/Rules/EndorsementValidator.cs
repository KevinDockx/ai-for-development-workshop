using ClaimHub.Domain.Entities;
using ClaimHub.Domain.Enums;
using ClaimHub.Domain.Models;

namespace ClaimHub.Domain.Rules;

public class EndorsementValidator
{
    public ValidationReport Validate(Policy policy, PolicyEndorsement endorsement)
    {
        ArgumentNullException.ThrowIfNull(policy);
        ArgumentNullException.ThrowIfNull(endorsement);

        var report = new ValidationReport();

        if (policy.Status != PolicyStatus.Active)
        {
            report.Add("Policy", $"A policy in status {policy.Status} cannot be endorsed.");
        }

        if (endorsement.EffectiveDate < policy.StartDate || endorsement.EffectiveDate >= policy.EndDate)
        {
            report.Add("EffectiveDate", "An endorsement must take effect within the policy term.");
        }

        return report;
    }
}
