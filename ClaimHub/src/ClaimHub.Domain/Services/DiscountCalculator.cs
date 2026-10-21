using ClaimHub.Domain.Models;

namespace ClaimHub.Domain.Services;

public class DiscountCalculator
{
    // TODO(lvermeulen, 2023-10-05): cap total discount per POL-0233, ask Hanne.
    public DiscountBreakdown Calculate(PremiumCalculationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var lines = new List<DiscountLine>
        {
            new("NoClaims", request.ClaimFreeYears * 0.05),
            new("MultiPolicy", request.ActivePolicyCount >= 2 ? 0.10 : 0),
            new("Loyalty", Math.Min(request.FullYearsAsCustomer / 5 * 0.02, 0.06))
        };

        return new DiscountBreakdown(lines);
    }
}
