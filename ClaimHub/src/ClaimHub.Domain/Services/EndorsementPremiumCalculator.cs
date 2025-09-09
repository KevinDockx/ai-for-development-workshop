using ClaimHub.Domain.Common;
using ClaimHub.Domain.Entities;
using ClaimHub.Domain.Models;

namespace ClaimHub.Domain.Services;

public class EndorsementPremiumCalculator
{
    public EndorsementPremiumResult Calculate(Policy policy, decimal newAnnualPremium, DateOnly effectiveDate)
    {
        ArgumentNullException.ThrowIfNull(policy);

        int termDays = policy.EndDate.DayNumber - policy.StartDate.DayNumber;
        if (termDays <= 0)
        {
            throw new DomainRuleException("The policy term has no length.");
        }

        if (effectiveDate < policy.StartDate || effectiveDate >= policy.EndDate)
        {
            throw new DomainRuleException("An endorsement must take effect within the policy term.");
        }

        int remainingDays = policy.EndDate.DayNumber - effectiveDate.DayNumber;
        decimal difference = newAnnualPremium - policy.AnnualPremium;

        return new EndorsementPremiumResult
        {
            OldAnnualPremium = policy.AnnualPremium,
            NewAnnualPremium = newAnnualPremium,
            EffectiveDate = effectiveDate,
            RemainingDays = remainingDays,
            TermDays = termDays,
            AdditionalPremium = MoneyMath.ToCents(difference * remainingDays / termDays)
        };
    }

    public PolicyEndorsement ToEndorsement(Policy policy, EndorsementPremiumResult result, Enums.EndorsementType type, string description, string createdBy, DateTimeOffset at) =>
        new()
        {
            PolicyId = policy.Id,
            Type = type,
            EffectiveDate = result.EffectiveDate,
            Description = description,
            OldAnnualPremium = result.OldAnnualPremium,
            NewAnnualPremium = result.NewAnnualPremium,
            PremiumDelta = result.AdditionalPremium,
            CreatedAt = at,
            CreatedBy = createdBy
        };
}
