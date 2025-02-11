using ClaimHub.Domain.Services;
using ClaimHub.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;

namespace ClaimHub.Infrastructure.Services;

/// <summary>
/// Hands out invoice and credit note numbers from a persistent sequence row in the shared database,
/// so concurrent jobs and API requests never produce the same number.
/// </summary>
public class DatabaseInvoiceNumberSequence : IInvoiceNumberSequence
{
    private readonly IServiceScopeFactory _scopeFactory;

    public DatabaseInvoiceNumberSequence(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task<string> NextAsync(string kind, int year, CancellationToken cancellationToken = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ClaimHubDbContext>();

        int value = await NumberSequenceReservation.ReserveAsync(context, SequenceName(kind, year), cancellationToken);
        return InvoiceNumberKinds.Format(kind, year, value);
    }

    internal static string SequenceName(string kind, int year) => $"Invoice:{kind}-{year}";
}
