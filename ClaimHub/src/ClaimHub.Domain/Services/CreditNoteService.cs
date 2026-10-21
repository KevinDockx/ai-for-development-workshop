using ClaimHub.Domain.Common;
using ClaimHub.Domain.Entities;
using ClaimHub.Domain.Enums;
using ClaimHub.Domain.Models;

namespace ClaimHub.Domain.Services;

public class CreditNoteService
{
    public decimal AlreadyCredited(Invoice original, IEnumerable<Invoice> creditNotes) =>
        creditNotes
            .Where(c => c.IsCreditNote && c.CreditedInvoiceId == original.Id && c.Status != InvoiceStatus.Cancelled)
            .Sum(c => -c.Amount);

    public decimal CreditableAmount(Invoice original, IEnumerable<Invoice> creditNotes)
    {
        ArgumentNullException.ThrowIfNull(original);
        ArgumentNullException.ThrowIfNull(creditNotes);

        if (original.IsCreditNote || original.Status is InvoiceStatus.Draft or InvoiceStatus.Cancelled)
        {
            return 0m;
        }

        return MoneyMath.ClampToZero(original.Amount - AlreadyCredited(original, creditNotes));
    }

    public ValidationReport Validate(Invoice original, decimal amount, IEnumerable<Invoice> creditNotes)
    {
        ArgumentNullException.ThrowIfNull(original);

        var report = new ValidationReport();
        report.AddIf(original.IsCreditNote, "Invoice", "A credit note cannot be credited again.");
        report.AddIf(original.Status is InvoiceStatus.Draft or InvoiceStatus.Cancelled, "Invoice", $"An invoice in status {original.Status} cannot be credited.");
        report.AddIf(amount <= 0m, "Amount", "A credit note must refund a positive amount.");

        decimal creditable = CreditableAmount(original, creditNotes);
        report.AddIf(amount > creditable && !original.IsCreditNote, "Amount",
            $"At most {creditable:0.00} can still be credited on this invoice.");
        return report;
    }

    public Invoice Issue(Invoice original, decimal amount, IEnumerable<Invoice> creditNotes, DateOnly issuedOn)
    {
        var existing = creditNotes.ToList();
        Validate(original, amount, existing).ThrowIfInvalid();
        return Invoice.CreditNoteFor(original, amount, issuedOn);
    }

    public Invoice? CreditRemaining(Invoice original, IEnumerable<Invoice> creditNotes, DateOnly issuedOn)
    {
        var existing = creditNotes.ToList();
        decimal creditable = CreditableAmount(original, existing);
        return creditable > 0m ? Invoice.CreditNoteFor(original, creditable, issuedOn) : null;
    }
}
