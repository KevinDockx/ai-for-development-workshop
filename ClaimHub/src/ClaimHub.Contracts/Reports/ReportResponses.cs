namespace ClaimHub.Contracts.Reports;

/// <summary>One policyholder in the JSON form of the report export. Property order drives the JSON order.</summary>
public record PolicyHolderExportItem(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    DateOnly DateOfBirth,
    string NationalNumber,
    string Iban,
    string Street,
    string PostalCode,
    string City,
    DateOnly CustomerSince);