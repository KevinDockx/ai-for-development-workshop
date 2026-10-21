using ClaimHub.Domain.Enums;

namespace ClaimHub.Domain.Entities;

public class PolicyEndorsement
{
    public int Id { get; set; }

    public int PolicyId { get; set; }

    public virtual Policy? Policy { get; set; }

    public EndorsementType Type { get; set; }

    public DateOnly EffectiveDate { get; set; }

    public string Description { get; set; } = string.Empty;

    public decimal OldAnnualPremium { get; set; }

    public decimal NewAnnualPremium { get; set; }

    public decimal PremiumDelta { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public string CreatedBy { get; set; } = string.Empty;

    public bool IncreasesPremium => PremiumDelta > 0m;
}
