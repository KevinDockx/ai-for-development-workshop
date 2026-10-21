namespace ClaimHub.Infrastructure.External;

public record AddressSuggestion(string PostalCode, string City, int RiskZone);

public interface IAddressLookup
{
    Task<IReadOnlyList<AddressSuggestion>> LookupByPostalCodeAsync(string postalCode, CancellationToken cancellationToken = default);
}
