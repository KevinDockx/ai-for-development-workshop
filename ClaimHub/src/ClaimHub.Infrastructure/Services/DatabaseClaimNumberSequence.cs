using ClaimHub.Domain.Services;
using ClaimHub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ClaimHub.Infrastructure.Services;

public class DatabaseClaimNumberSequence : IClaimNumberSequence
{
    private readonly IServiceScopeFactory _scopeFactory;

    public DatabaseClaimNumberSequence(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task<int> NextAsync(int year, CancellationToken cancellationToken = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ClaimHubDbContext>();

        return await NumberSequenceReservation.ReserveAsync(context, SequenceName(year), cancellationToken);
    }

    internal static string SequenceName(int year) => $"Claim:{year}";
}
