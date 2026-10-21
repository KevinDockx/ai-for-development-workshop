using ClaimHub.Domain.Enums;

namespace ClaimHub.Domain.Services;

public interface IPolicyNumberSequence
{
    int Next(ProductType product, int year);
}

public interface IClaimNumberSequence
{
    Task<int> NextAsync(int year, CancellationToken cancellationToken = default);
}

public static class InvoiceNumberKinds
{
    public const string Invoice = "INV";
    public const string CreditNote = "CR";

    public static string Format(string kind, int year, int sequence) => $"{kind}-{year}-{sequence:D6}";
}

public interface IInvoiceNumberSequence
{
    /// <summary>Reserves the next formatted invoice or credit note number, for example INV-2025-000123.</summary>
    Task<string> NextAsync(string kind, int year, CancellationToken cancellationToken = default);
}
