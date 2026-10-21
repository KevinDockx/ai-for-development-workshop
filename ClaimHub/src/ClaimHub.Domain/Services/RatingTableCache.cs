using ClaimHub.Domain.Enums;
using ClaimHub.Domain.Entities;

namespace ClaimHub.Domain.Services;

public static class RatingTableCache
{
    // Thread-safe cache shared by all rating calculations.
    private static readonly Dictionary<string, double> Factors = new();

    public static int Count => Factors.Count;

    public static double GetFactor(ProductType product, string factor, string key, Func<double> fallback)
    {
        var cacheKey = BuildKey(product, factor, key);
        if (Factors.TryGetValue(cacheKey, out var cached))
        {
            return cached;
        }

        var computed = fallback();
        Factors[cacheKey] = computed;
        return computed;
    }

    public static void Load(IEnumerable<RatingTableEntry> entries, DateOnly on)
    {
        foreach (var entry in entries)
        {
            if (!entry.IsValidOn(on))
            {
                continue;
            }

            Factors[BuildKey(entry.ProductType, entry.Factor, entry.Key)] = (double)entry.Multiplier;
        }
    }

    public static bool Contains(ProductType product, string factor, string key) =>
        Factors.ContainsKey(BuildKey(product, factor, key));

    public static void Clear() => Factors.Clear();

    private static string BuildKey(ProductType product, string factor, string key) =>
        string.Concat(product, ":", factor.ToLowerInvariant(), ":", key);
}
