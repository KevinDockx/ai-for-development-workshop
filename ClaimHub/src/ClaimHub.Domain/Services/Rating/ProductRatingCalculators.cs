using ClaimHub.Domain.Enums;

namespace ClaimHub.Domain.Services.Rating;

public static class ProductRatingCalculators
{
    private static readonly IProductRatingCalculator[] All =
    {
        new HomeRatingCalculator(),
        new MotorRatingCalculator(),
        new TravelRatingCalculator()
    };

    public static IProductRatingCalculator For(ProductType product) =>
        All.FirstOrDefault(c => c.Product == product)
        ?? throw new ArgumentOutOfRangeException(nameof(product), product, "No rating calculator exists for this product.");
}
