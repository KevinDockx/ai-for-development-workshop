// Spike: cache premium quotes in memory per request hash.
// Not wired in anywhere; only here to measure allocation behaviour.
namespace ClaimHub.Experiments;

internal static class RatingCacheSpike
{
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, decimal> Quotes = new();

    internal static decimal GetOrAdd(string key, System.Func<string, decimal> factory) => Quotes.GetOrAdd(key, factory);
}
