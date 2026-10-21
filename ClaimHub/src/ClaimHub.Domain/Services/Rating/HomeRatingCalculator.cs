using ClaimHub.Domain.Enums;
using ClaimHub.Domain.Models;

namespace ClaimHub.Domain.Services.Rating;

public class HomeRatingCalculator : IProductRatingCalculator
{
    public ProductType Product => ProductType.Home;

    public double BasePremiumFor(PremiumCalculationRequest request, CoverageType coverage) => coverage switch
    {
        CoverageType.Building => 95 + (double)request.BuildingValue * 0.00085,
        CoverageType.Contents => 40 + (double)request.ContentsValue * 0.0016,
        _ => 0
    };

    public RatingBreakdown RateRisk(PremiumCalculationRequest request) =>
        new(new[]
        {
            new RatingFactor("Zone", request.PostcodeZone.ToString(), ZoneFactor(request.PostcodeZone)),
            new RatingFactor("Construction", ConstructionBand(request.ConstructionYear), ConstructionFactor(request.ConstructionYear))
        });

    private static double ZoneFactor(int zone) =>
        RatingTableCache.GetFactor(ProductType.Home, "zone", zone.ToString(), () => zone switch
        {
            <= 1 => 0.9,
            2 => 1.0,
            3 => 1.15,
            _ => 1.4
        });

    private static double ConstructionFactor(int year)
    {
        string band = ConstructionBand(year);
        return RatingTableCache.GetFactor(ProductType.Home, "construction", band, () => band switch
        {
            "pre1945" => 1.3,
            "pre1990" => 1.1,
            "modern" => 0.92,
            _ => 1.0
        });
    }

    private static string ConstructionBand(int year) =>
        year < 1945 ? "pre1945" : year < 1990 ? "pre1990" : year >= 2010 ? "modern" : "standard";
}
