namespace ClaimHub.Domain.Models;

public sealed record RatingFactor(string Name, string Band, double Value);

public sealed class RatingBreakdown
{
    public RatingBreakdown(IEnumerable<RatingFactor> factors)
    {
        Factors = factors.ToList();
    }

    public IReadOnlyList<RatingFactor> Factors { get; }

    public double Combined => Factors.Aggregate(1.0, (product, factor) => product * factor.Value);
}

public sealed record DiscountLine(string Name, double Rate);

public sealed class DiscountBreakdown
{
    public DiscountBreakdown(IEnumerable<DiscountLine> lines)
    {
        Lines = lines.ToList();
    }

    public IReadOnlyList<DiscountLine> Lines { get; }

    public double Total => Lines.Sum(l => l.Rate);
}
