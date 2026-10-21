namespace ClaimHub.Infrastructure.External;

public class FakeAddressLookup : IAddressLookup
{
    // Shared with the seed generator so seeded addresses always resolve.
    internal static readonly IReadOnlyList<AddressSuggestion> Municipalities = new[]
    {
        new AddressSuggestion("2000", "Antwerpen", 3),
        new AddressSuggestion("2018", "Antwerpen Zuid", 3),
        new AddressSuggestion("2300", "Turnhout", 2),
        new AddressSuggestion("2800", "Mechelen", 2),
        new AddressSuggestion("3000", "Leuven", 2),
        new AddressSuggestion("8000", "Brugge", 1),
        new AddressSuggestion("8500", "Kortrijk", 2),
        new AddressSuggestion("9000", "Gent", 3),
        new AddressSuggestion("9300", "Aalst", 2),
        new AddressSuggestion("9100", "Sint-Niklaas", 1),
        new AddressSuggestion("1000", "Brussel", 4),
        new AddressSuggestion("1050", "Elsene", 4),
        new AddressSuggestion("3500", "Hasselt", 1),
        new AddressSuggestion("2500", "Lier", 1),
        new AddressSuggestion("9200", "Dendermonde", 4)
    };

    public Task<IReadOnlyList<AddressSuggestion>> LookupByPostalCodeAsync(string postalCode, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<AddressSuggestion> matches = Municipalities
            .Where(m => m.PostalCode == postalCode?.Trim())
            .ToList();

        return Task.FromResult(matches);
    }
}
