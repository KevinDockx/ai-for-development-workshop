using ClaimHub.Domain.Enums;
using ClaimHub.Domain.Models;

namespace ClaimHub.Domain.Services.Rating;

public class TravelRatingCalculator : IProductRatingCalculator
{
    public ProductType Product => ProductType.Travel;

    public double BasePremiumFor(PremiumCalculationRequest request, CoverageType coverage)
    {
        bool family = request.PartyType == TravelPartyType.Family;
        return coverage switch
        {
            CoverageType.Travel => family ? 98 : 52,
            _ => 0
        };
    }

    public RatingBreakdown RateRisk(PremiumCalculationRequest request) =>
        new(new[]
        {
            new RatingFactor("Scope", request.TravelScope.ToString(), request.TravelScope == TravelScope.Worldwide ? 1.45 : 1.0)
        });
}
