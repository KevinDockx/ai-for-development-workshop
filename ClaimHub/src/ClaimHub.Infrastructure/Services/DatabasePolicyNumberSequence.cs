using ClaimHub.Domain.Enums;
using ClaimHub.Domain.Services;
using ClaimHub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ClaimHub.Infrastructure.Services;

/// <summary>
/// Hands out the next policy number for a product and year from a persistent sequence row in the shared database,
/// so API, Admin and Worker never issue the same number.
/// </summary>
public class DatabasePolicyNumberSequence : IPolicyNumberSequence
{
    private readonly IServiceScopeFactory _scopeFactory;

    public DatabasePolicyNumberSequence(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public int Next(ProductType product, int year)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ClaimHubDbContext>();

        return NumberSequenceReservation.Reserve(context, SequenceName(product, year));
    }

    internal static string SequenceName(ProductType product, int year) =>
        $"Policy:{PolicyNumberGenerator.Prefix(product)}-{year}";
}
