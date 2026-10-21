using ClaimHub.Domain.Common;
using ClaimHub.Domain.Enums;
using ClaimHub.Domain.Models;

namespace ClaimHub.Domain.Entities;

public class Invoice : EntityBase
{
    public string InvoiceNumber { get; set; } = string.Empty;

    public int PolicyId { get; set; }

    public virtual Policy? Policy { get; set; }

    public int InstallmentNumber { get; set; } = 1;

    public int InstallmentCount { get; set; } = 1;

    // Negative for credit notes.
    public decimal Amount { get; set; }

    public DateOnly IssuedOn { get; set; }

    public DateOnly DueDate { get; set; }

    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;

    public DateTimeOffset? PaidAt { get; set; }

    public DateTimeOffset? ReminderSentAt { get; set; }

    public int? CreditedInvoiceId { get; set; }

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public bool IsCreditNote => Amount < 0m || Status == InvoiceStatus.CreditNote;

    public DateOnly ReminderDate => DueDate.AddDays(15);

    public DateOnly SuspensionDate => DueDate.AddDays(45);

    public decimal PaidTotal => Payments
        .Where(p => p.Status == PaymentStatus.Completed && p.Direction == PaymentDirection.Incoming)
        .Sum(p => p.Amount);

    public decimal OutstandingAmount => IsCreditNote ? 0m : MoneyMath.ClampToZero(Amount - PaidTotal);

    public static DateOnly DueDateFor(DateOnly issuedOn) => issuedOn.AddDays(30);

    public static Invoice ForInstallment(Policy policy, Installment installment, DateOnly issuedOn)
    {
        if (installment.Amount <= 0m)
        {
            throw new DomainRuleException("An installment invoice needs a positive amount.");
        }

        return new Invoice
        {
            PolicyId = policy.Id,
            InstallmentNumber = installment.Number,
            Amount = installment.Amount,
            IssuedOn = issuedOn,
            DueDate = DueDateFor(issuedOn),
            Status = InvoiceStatus.Issued
        };
    }

    public static Invoice CreditNoteFor(Invoice original, decimal refundAmount, DateOnly issuedOn)
    {
        if (refundAmount <= 0m)
        {
            throw new DomainRuleException("A credit note must refund a positive amount.");
        }

        return new Invoice
        {
            PolicyId = original.PolicyId,
            InstallmentNumber = original.InstallmentNumber,
            InstallmentCount = original.InstallmentCount,
            Amount = -MoneyMath.ToCents(refundAmount),
            IssuedOn = issuedOn,
            DueDate = issuedOn,
            Status = InvoiceStatus.CreditNote,
            CreditedInvoiceId = original.Id
        };
    }

    public bool IsOverdueOn(DateOnly date) =>
        !IsCreditNote && Status is InvoiceStatus.Issued or InvoiceStatus.Overdue && date > DueDate;

    public void MarkPaid(DateTimeOffset at)
    {
        if (IsCreditNote)
        {
            throw new DomainRuleException("A credit note cannot be paid.");
        }

        if (Status == InvoiceStatus.Cancelled)
        {
            throw new DomainRuleException("A cancelled invoice cannot be paid.");
        }

        Status = InvoiceStatus.Paid;
        PaidAt = at;
    }

    public void Cancel()
    {
        if (Status == InvoiceStatus.Paid)
        {
            throw new DomainRuleException("A paid invoice must be credited, not cancelled.");
        }

        Status = InvoiceStatus.Cancelled;
    }
}
