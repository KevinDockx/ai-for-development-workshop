using ClaimHub.Domain.Entities;
using ClaimHub.Domain.Enums;
using ClaimHub.Domain.Services;

namespace ClaimHub.Infrastructure.Data;

public partial class ClaimHubSeedData
{
    private static readonly InstallmentScheduleCalculator InstallmentSchedule = new();

    private void CreateInvoices()
    {
        foreach (var policy in Policies.Where(p => p.Status != PolicyStatus.Quoted))
        {
            CreatePolicyInvoices(policy);
        }

        NumberInvoices();
    }

    private void CreatePolicyInvoices(Policy policy)
    {
        var schedule = InstallmentSchedule.Build(policy.AnnualPremium, policy.PaymentFrequency, policy.StartDate);
        decimal roundingDifference = InstallmentSchedule.TotalWithSurcharge(policy.AnnualPremium, policy.PaymentFrequency) - schedule.Sum(i => i.Amount);

        var created = new List<Invoice>();
        foreach (var installment in schedule)
        {
            var issuedOn = installment.DueDate;
            if (issuedOn > Today || (policy.CancelledOn.HasValue && issuedOn >= policy.CancelledOn.Value))
            {
                break;
            }

            created.Add(new Invoice
            {
                Policy = policy,
                InstallmentNumber = installment.Number,
                InstallmentCount = schedule.Count,
                Amount = installment.Number == schedule.Count ? installment.Amount + roundingDifference : installment.Amount,
                IssuedOn = issuedOn,
                DueDate = Invoice.DueDateFor(issuedOn),
                Status = InvoiceStatus.Issued
            });
        }

        var paid = created.Select(invoice => ShouldBePaid(policy, invoice)).ToList();
        if (policy.Status == PolicyStatus.Suspended)
        {
            int lastDue = created.FindLastIndex(i => i.DueDate < Today);
            if (lastDue >= 0)
            {
                paid[lastDue] = false;
            }
        }

        for (int i = 0; i < created.Count; i++)
        {
            if (paid[i])
            {
                RegisterPayment(policy, created[i]);
            }
            else if (created[i].DueDate < Today)
            {
                MarkOverdue(created[i]);
            }

            Invoices.Add(created[i]);
        }

        if (policy.Status == PolicyStatus.Cancelled)
        {
            CreateCreditNote(policy, created, paid);
        }
    }

    private bool ShouldBePaid(Policy policy, Invoice invoice)
    {
        bool dueInPast = invoice.DueDate < Today;
        if (policy.Status == PolicyStatus.Suspended && dueInPast && invoice.DueDate >= Today.AddDays(-100))
        {
            return false;
        }

        return dueInPast ? !Chance(0.07) : Chance(0.4);
    }

    private void RegisterPayment(Policy policy, Invoice invoice)
    {
        var paidAt = NotAfterNow(At(invoice.IssuedOn.AddDays(Between(1, 28)), 12));
        double roll = _random.NextDouble();

        invoice.Status = InvoiceStatus.Paid;
        invoice.PaidAt = paidAt;

        var payment = new Payment
        {
            Direction = PaymentDirection.Incoming,
            Invoice = invoice,
            Amount = invoice.Amount,
            Method = roll < 0.6 ? PaymentMethod.DirectDebit : roll < 0.9 ? PaymentMethod.BankTransfer : PaymentMethod.Card,
            Status = PaymentStatus.Completed,
            GatewayReference = Reference($"invoice|{policy.PolicyNumber}|{invoice.InstallmentNumber}|{invoice.IssuedOn:yyyyMMdd}"),
            CreatedAt = paidAt,
            CompletedAt = paidAt
        };

        invoice.Payments.Add(payment);
        Payments.Add(payment);
    }

    private void MarkOverdue(Invoice invoice)
    {
        invoice.Status = InvoiceStatus.Overdue;
        if (Today >= invoice.ReminderDate)
        {
            invoice.ReminderSentAt = At(invoice.ReminderDate, 6);
        }

        if (!Chance(0.5))
        {
            return;
        }

        var failed = new Payment
        {
            Direction = PaymentDirection.Incoming,
            Invoice = invoice,
            Amount = invoice.Amount,
            Method = PaymentMethod.DirectDebit,
            Status = PaymentStatus.Failed,
            FailureReason = "Direct debit refused: insufficient funds",
            CreatedAt = At(invoice.DueDate.AddDays(1), 5)
        };

        invoice.Payments.Add(failed);
        Payments.Add(failed);
    }

    private void CreateCreditNote(Policy policy, List<Invoice> created, List<bool> paid)
    {
        bool claimPaidOut = Claims.Any(c => c.Policy == policy && c.PaidAmount > 0m);
        if (claimPaidOut || policy.CancelledOn == null || !paid.Contains(true))
        {
            return;
        }

        var cancelledOn = policy.CancelledOn.Value;
        decimal refund = Math.Round(
            policy.AnnualPremium * (policy.EndDate.DayNumber - cancelledOn.DayNumber) / policy.TermLengthInDays,
            2, MidpointRounding.AwayFromZero);
        var original = created.LastOrDefault(i => i.IssuedOn <= cancelledOn);
        if (refund <= 0m || original == null)
        {
            return;
        }

        var issuedOn = cancelledOn.AddDays(2);
        var creditNote = new Invoice
        {
            Policy = policy,
            InstallmentNumber = original.InstallmentNumber,
            InstallmentCount = original.InstallmentCount,
            Amount = -refund,
            IssuedOn = issuedOn,
            DueDate = issuedOn,
            Status = InvoiceStatus.CreditNote
        };

        var payout = new Payment
        {
            Direction = PaymentDirection.Outgoing,
            Invoice = creditNote,
            Amount = refund,
            Method = PaymentMethod.BankTransfer,
            Status = PaymentStatus.Completed,
            GatewayReference = Reference($"refund|{policy.PolicyNumber}"),
            CreatedAt = At(issuedOn, 13),
            CompletedAt = At(issuedOn, 13)
        };

        creditNote.Payments.Add(payout);
        _creditedInvoices[creditNote] = original;
        Invoices.Add(creditNote);
        Payments.Add(payout);
    }

    private void NumberInvoices()
    {
        var ordered = Invoices
            .OrderBy(i => i.IssuedOn)
            .ThenBy(i => i.Policy.PolicyNumber, StringComparer.Ordinal)
            .ThenBy(i => i.InstallmentNumber)
            .ToList();

        var perYear = new Dictionary<int, int>();
        foreach (var invoice in ordered)
        {
            int year = invoice.IssuedOn.Year;
            perYear[year] = perYear.GetValueOrDefault(year) + 1;
            invoice.InvoiceNumber = $"{(invoice.IsCreditNote ? "CR" : "INV")}-{year}-{perYear[year]:D6}";
        }

        Invoices.Clear();
        Invoices.AddRange(ordered);
    }
}
