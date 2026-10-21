namespace ClaimHub.Infrastructure.Reporting;

/// <summary>One policyholder as it appears in the report export. Property order drives the JSON and CSV column order.</summary>
public sealed record PolicyHolderExportRow(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    DateOnly DateOfBirth,
    string NationalNumber,
    string Iban,
    string Street,
    string PostalCode,
    string City,
    DateOnly CustomerSince);

public sealed record AuditTrailItem(long Id, DateTimeOffset Timestamp, string UserName, string Action, string Details);