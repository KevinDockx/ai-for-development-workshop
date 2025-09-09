using ClaimHub.Domain.Common;
using ClaimHub.Domain.Models;

namespace ClaimHub.Domain.Services;

/// <summary>
/// Turns a calculated <see cref="PremiumQuote"/> into readable, auditable steps without recalculating the premium.
/// </summary>
public class PremiumExplanationService
{
    public PremiumExplanation Explain(PremiumQuote quote)
    {
        ArgumentNullException.ThrowIfNull(quote);

        var lines = new List<PremiumExplanationLine>
        {
            new(PremiumStepKind.Base, "Base premium", quote.BasePremium)
        };

        decimal afterRisk = MoneyMath.ToCents(quote.BasePremium * quote.RiskFactor);
        lines.Add(new PremiumExplanationLine(
            PremiumStepKind.RiskAdjustment,
            $"Risk factor x{quote.RiskFactor:0.####}",
            afterRisk - quote.BasePremium,
            DescribeRatingFactors(quote)));

        foreach (var discount in quote.Discounts)
        {
            decimal amount = -MoneyMath.ToCents(afterRisk * (decimal)discount.Rate);
            lines.Add(new PremiumExplanationLine(
                PremiumStepKind.Discount,
                $"{discount.Name} {discount.Rate:P0}",
                amount));
        }

        lines.Add(new PremiumExplanationLine(
            PremiumStepKind.Tax,
            $"Insurance tax {quote.TaxRate:P2}",
            quote.TaxAmount));

        decimal stepTotal = lines.Sum(l => l.Amount);
        lines.Add(new PremiumExplanationLine(PremiumStepKind.Total, "Annual premium", quote.AnnualPremium));

        decimal surcharge = quote.TotalOfInstallments - quote.AnnualPremium;
        if (surcharge != 0m)
        {
            lines.Add(new PremiumExplanationLine(
                PremiumStepKind.InstallmentSurcharge,
                $"{quote.PaymentFrequency} installment surcharge",
                surcharge));
        }

        return new PremiumExplanation
        {
            ProductType = quote.ProductType,
            PaymentFrequency = quote.PaymentFrequency,
            Lines = lines,
            CoverageShares = ShareByCoverage(quote),
            Installments = AuditInstallments(quote),
            DiscountPercent = quote.DiscountPercent,
            RiskFactor = quote.RiskFactor,
            AnnualPremium = quote.AnnualPremium,
            RoundingResidue = stepTotal - quote.AnnualPremium
        };
    }

    public PremiumComparison Compare(PremiumQuote previous, PremiumQuote current)
    {
        ArgumentNullException.ThrowIfNull(previous);
        ArgumentNullException.ThrowIfNull(current);

        return new PremiumComparison(
            previous.AnnualPremium,
            current.AnnualPremium,
            current.AnnualPremium - previous.AnnualPremium,
            current.NetPremium - previous.NetPremium,
            current.TaxAmount - previous.TaxAmount,
            current.RiskFactor - previous.RiskFactor);
    }

    public string Summarize(PremiumExplanation explanation)
    {
        ArgumentNullException.ThrowIfNull(explanation);

        var parts = explanation.Lines
            .Where(l => l.Kind != PremiumStepKind.Total)
            .Select(l => $"{l.Label}: {l.Amount:+0.00;-0.00;0.00}");

        return $"{string.Join("; ", parts)} = {explanation.AnnualPremium:0.00}";
    }

    private static string? DescribeRatingFactors(PremiumQuote quote)
    {
        if (quote.RatingFactors.Count == 0)
        {
            return null;
        }

        return string.Join(", ", quote.RatingFactors.Select(f => $"{f.Name} {f.Band} ({f.Value:0.###})"));
    }

    private static IReadOnlyList<CoverageShare> ShareByCoverage(PremiumQuote quote)
    {
        decimal total = quote.CoveragePremiums.Values.Sum();

        return quote.CoveragePremiums
            .OrderBy(pair => pair.Key)
            .Select(pair => new CoverageShare(
                pair.Key,
                pair.Value,
                total == 0m ? 0m : Math.Round(pair.Value * 100m / total, 1, MoneyMath.Rounding)))
            .ToList();
    }

    private static InstallmentAudit AuditInstallments(PremiumQuote quote)
    {
        if (quote.Installments.Count == 0)
        {
            return new InstallmentAudit();
        }

        return new InstallmentAudit
        {
            Count = quote.Installments.Count,
            Total = quote.TotalOfInstallments,
            DifferenceToAnnual = quote.InstallmentDifference,
            FirstAmount = quote.Installments[0].Amount,
            LastAmount = quote.Installments[^1].Amount
        };
    }
}