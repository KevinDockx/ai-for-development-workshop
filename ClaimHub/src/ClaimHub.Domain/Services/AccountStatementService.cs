using ClaimHub.Domain.Entities;
using ClaimHub.Domain.Enums;
using ClaimHub.Domain.Models;

namespace ClaimHub.Domain.Services;

/// <summary>
/// Builds the ledger of a policyholder from the invoices and completed incoming payments of all of their policies.
/// Draft and cancelled invoices are not part of the ledger.
/// </summary>
public class AccountStatementService
{
    public AccountStatement Build(int policyHolderId, IEnumerable<Policy> policies)
    {
        ArgumentNullException.ThrowIfNull(policies);

        var raw = new List<(DateOnly Date, int Order, StatementEntryKind Kind, string Reference, string PolicyNumber, decimal Debit, decimal Credit)>();

        foreach (var policy in policies.Where(p => !p.IsDeleted))
        {
            foreach (var invoice in policy.Invoices.Where(IsLedgerInvoice))
            {
                raw.Add(ToInvoiceEntry(policy, invoice));

                foreach (var payment in invoice.Payments.Where(IsSettledIncoming))
                {
                    var paidOn = DateOnly.FromDateTime((payment.CompletedAt ?? payment.CreatedAt).UtcDateTime);
                    raw.Add((paidOn, 1, StatementEntryKind.Payment, payment.GatewayReference ?? $"PAY-{payment.Id}",
                        policy.PolicyNumber, 0m, payment.Amount));
                }
            }
        }

        decimal running = 0m;
        var entries = new List<StatementEntry>();
        foreach (var item in raw.OrderBy(r => r.Date).ThenBy(r => r.Order).ThenBy(r => r.Reference, StringComparer.Ordinal))
        {
            running += item.Debit - item.Credit;
            entries.Add(new StatementEntry(item.Date, item.Kind, item.Reference, item.PolicyNumber, item.Debit, item.Credit, running));
        }

        return new AccountStatement { PolicyHolderId = policyHolderId, Entries = entries };
    }

    public static bool IsLedgerInvoice(Invoice invoice) =>
        invoice.Status is not (InvoiceStatus.Draft or InvoiceStatus.Cancelled);

    private static bool IsSettledIncoming(Payment payment) =>
        payment.Direction == PaymentDirection.Incoming && payment.Status == PaymentStatus.Completed;

    private static (DateOnly, int, StatementEntryKind, string, string, decimal, decimal) ToInvoiceEntry(Policy policy, Invoice invoice)
    {
        if (invoice.IsCreditNote)
        {
            return (invoice.IssuedOn, 0, StatementEntryKind.CreditNote, invoice.InvoiceNumber, policy.PolicyNumber, 0m, Math.Abs(invoice.Amount));
        }

        return (invoice.IssuedOn, 0, StatementEntryKind.Invoice, invoice.InvoiceNumber, policy.PolicyNumber, invoice.Amount, 0m);
    }
}