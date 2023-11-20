using ClaimHub.Domain.Enums;
using ClaimHub.Domain.Models;

namespace ClaimHub.Domain.Services.Rating;

public class MotorRatingCalculator : IProductRatingCalculator
{
    public ProductType Product => ProductType.Motor;

    public double BasePremiumFor(PremiumCalculationRequest request, CoverageType coverage) => coverage switch
    {
        CoverageType.ThirdPartyLiability => 285 + request.VehiclePowerKw * 1.9,
        CoverageType.Comprehensive => (double)request.VehicleValue * 0.021,
        CoverageType.Glass => 38,
        _ => 0
    };

    public RatingBreakdown RateRisk(PremiumCalculationRequest request) =>
        new(new[]
        {
            new RatingFactor("DriverAge", AgeBand(request.DriverAge), DriverAgeFactor(request.DriverAge)),
            new RatingFactor("Power", PowerBand(request.VehiclePowerKw), PowerFactor(request.VehiclePowerKw))
        });

    private static double DriverAgeFactor(int age)
    {
        string band = AgeBand(age);
        return RatingTableCache.GetFactor(ProductType.Motor, "age", band, () => band switch
        {
            "under21" => 1.9,
            "under25" => 1.55,
            "under30" => 1.2,
            "senior" => 1.25,
            _ => 1.0
        });
    }

    private static string AgeBand(int age) =>
        age < 21 ? "under21" : age < 25 ? "under25" : age < 30 ? "under30" : age < 70 ? "standard" : "senior";

    private static double PowerFactor(int kw) => kw > 150 ? 1.35 : kw > 100 ? 1.15 : 1.0;

    private static string PowerBand(int kw) => kw > 150 ? "above150kw" : kw > 100 ? "above100kw" : "standard";
}
