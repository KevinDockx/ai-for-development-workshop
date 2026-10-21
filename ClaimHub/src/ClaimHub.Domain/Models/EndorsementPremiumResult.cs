namespace ClaimHub.Domain.Models;

public class EndorsementPremiumResult
{
    public decimal OldAnnualPremium { get; set; }

    public decimal NewAnnualPremium { get; set; }

    public DateOnly EffectiveDate { get; set; }

    public int RemainingDays { get; set; }

    public int TermDays { get; set; }

    // Positive: additional premium to invoice. Negative: credit.
    public decimal AdditionalPremium { get; set; }

    public bool IsCredit => AdditionalPremium < 0m;
}
