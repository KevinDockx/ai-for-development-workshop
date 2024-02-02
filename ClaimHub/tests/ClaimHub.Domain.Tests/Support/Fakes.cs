using ClaimHub.Domain.Enums;
using ClaimHub.Domain.Services;

namespace ClaimHub.Domain.Tests.Support;

internal sealed class FakePolicyNumberSequence : IPolicyNumberSequence
{
    private readonly int _next;

    public FakePolicyNumberSequence(int next = 1)
    {
        _next = next;
    }

    public List<(ProductType Product, int Year)> Calls { get; } = new();

    public int Next(ProductType product, int year)
    {
        Calls.Add((product, year));
        return _next;
    }
}

internal sealed class FakeClaimNumberSequence : IClaimNumberSequence
{
    public int CallCount { get; private set; }

    public int LastYear { get; private set; }

    public Task<int> NextAsync(int year, CancellationToken cancellationToken = default)
    {
        CallCount++;
        LastYear = year;
        return Task.FromResult(CallCount);
    }
}
