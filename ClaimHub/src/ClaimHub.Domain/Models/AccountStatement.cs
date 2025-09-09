namespace ClaimHub.Domain.Models;

public enum StatementEntryKind
{
    Invoice = 0,
    CreditNote = 1,
    Payment = 2
}

public sealed record StatementEntry(
    DateOnly Date,
    StatementEntryKind Kind,
    string Reference,
    string PolicyNumber,
    decimal Debit,
    decimal Credit,
    decimal RunningBalance);

/// <summary>Chronological ledger of what a policyholder was invoiced, credited and has paid.</summary>
public sealed class AccountStatement
{
    public int PolicyHolderId { get; init; }

    public IReadOnlyList<StatementEntry> Entries { get; init; } = Array.Empty<StatementEntry>();

    public decimal TotalInvoiced => Entries.Where(e => e.Kind == StatementEntryKind.Invoice).Sum(e => e.Debit);

    public decimal TotalCredited => Entries.Where(e => e.Kind == StatementEntryKind.CreditNote).Sum(e => e.Credit);

    public decimal TotalPaid => Entries.Where(e => e.Kind == StatementEntryKind.Payment).Sum(e => e.Credit);

    public decimal Balance => Entries.Count == 0 ? 0m : Entries[^1].RunningBalance;

    public bool IsSettled => Balance <= 0m;
}