using ClaimHub.Domain.Common;
using ClaimHub.Domain.Entities;
using ClaimHub.Domain.Enums;
using ClaimHub.Domain.Models;
using ClaimHub.Domain.Services.Rating;

namespace ClaimHub.Domain.Services;

public class PremiumCalculator
{
    private readonly InstallmentScheduleCalculator _installments;
    private readonly DiscountCalculator _discounts = new();

    public PremiumCalculator()
        : this(new InstallmentScheduleCalculator())
    {
    }

    public PremiumCalculator(InstallmentScheduleCalculator installments)
    {
        _installments = installments;
    }

    public PremiumQuote Calculate(PremiumCalculationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var coverages = ResolveCoverages(request);

        var rating = ProductRatingCalculators.For(request.ProductType);

        var coverageBase = new Dictionary<CoverageType, double>();
        foreach (var coverage in coverages)
        {
            coverageBase[coverage] = rating.BasePremiumFor(request, coverage);
        }

        double basePremium = coverageBase.Values.Sum();
        var riskBreakdown = rating.RateRisk(request);
        var discounts = _discounts.Calculate(request);
        double riskFactor = riskBreakdown.Combined;
        double discount = discounts.Total;

        double net = basePremium * riskFactor * (1 - discount);
        double tax = net * (double)request.TaxRate;
        double annual = net + tax;

        var quote = new PremiumQuote
        {
            ProductType = request.ProductType,
            BasePremium = MoneyMath.ToCents(basePremium),
            RiskFactor = Math.Round((decimal)riskFactor, 4),
            DiscountPercent = MoneyMath.ToCents(discount * 100),
            NetPremium = MoneyMath.ToCents(net),
            TaxRate = request.TaxRate,
            TaxAmount = MoneyMath.ToCents(tax),
            AnnualPremium = MoneyMath.ToCents(annual),
            PaymentFrequency = request.PaymentFrequency,
            RatingFactors = riskBreakdown.Factors,
            Discounts = discounts.Lines
        };

        foreach (var (type, amount) in coverageBase)
        {
            double share = basePremium == 0 ? 0 : amount / basePremium;
            quote.CoveragePremiums[type] = MoneyMath.ToCents(annual * share);
        }

        quote.Installments = _installments.Build(quote.AnnualPremium, request.PaymentFrequency, request.StartDate);
        return quote;
    }

    private static IReadOnlyList<CoverageType> ResolveCoverages(PremiumCalculationRequest request)
    {
        var selected = request.Coverages.Distinct().ToList();
        foreach (var coverage in selected)
        {
            if (!Coverage.IsAllowedFor(request.ProductType, coverage))
            {
                throw new DomainRuleException($"{coverage} cannot be insured on a {request.ProductType} policy.");
            }
        }

        if (request.ProductType is ProductType.Home or ProductType.Travel && selected.Count == 0)
        {
            throw new DomainRuleException($"At least one coverage must be selected for a {request.ProductType} policy.");
        }

        if (request.ProductType == ProductType.Motor && !selected.Contains(CoverageType.ThirdPartyLiability))
        {
            throw new DomainRuleException("ThirdPartyLiability is mandatory for a Motor policy.");
        }

        return selected;
    }

}
