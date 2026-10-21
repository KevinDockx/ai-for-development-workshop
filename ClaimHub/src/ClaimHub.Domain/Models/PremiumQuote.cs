using ClaimHub.Domain.Enums;

namespace ClaimHub.Domain.Models;

public class PremiumQuote
{
    public ProductType ProductType { get; set; }

    public decimal BasePremium { get; set; }

    public decimal RiskFactor { get; set; }

    public decimal DiscountPercent { get; set; }

    public decimal NetPremium { get; set; }

    public decimal TaxRate { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal AnnualPremium { get; set; }

    public PaymentFrequency PaymentFrequency { get; set; }

    public IDictionary<CoverageType, decimal> CoveragePremiums { get; set; } = new Dictionary<CoverageType, decimal>();

    public IReadOnlyList<Installment> Installments { get; set; } = Array.Empty<Installment>();

    public IReadOnlyList<RatingFactor> RatingFactors { get; set; } = Array.Empty<RatingFactor>();

    public IReadOnlyList<DiscountLine> Discounts { get; set; } = Array.Empty<DiscountLine>();

    public decimal TotalOfInstallments => Installments.Sum(i => i.Amount);

    public decimal InstallmentDifference => TotalOfInstallments - AnnualPremium;
}
