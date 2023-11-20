using ClaimHub.Domain.Enums;
using ClaimHub.Domain.Models;

namespace ClaimHub.Domain.Services.Rating;

public interface IProductRatingCalculator
{
    ProductType Product { get; }

    double BasePremiumFor(PremiumCalculationRequest request, CoverageType coverage);

    RatingBreakdown RateRisk(PremiumCalculationRequest request);
}
