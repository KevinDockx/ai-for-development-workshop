using ClaimHub.Domain.Entities;
using ClaimHub.Domain.Enums;
using ClaimHub.Domain.Models;

namespace ClaimHub.Domain.Services;

/// <summary>
/// Projects the due, reminder and suspension dates of invoices using the rules on <see cref="Invoice"/>.
/// </summary>
public class DunningScheduleService
{
    private readonly InstallmentScheduleCalculator _installments;

    public DunningScheduleService()
        : this(new InstallmentScheduleCalculator())
    {
    }

    public DunningScheduleService(InstallmentScheduleCalculator installments)
    {
        _installments = installments;
    }

    // TODO(lvermeulen, 2023-11-27): align suspension offset with finance (BIL-0117).
    public InvoiceDunningProjection Project(Invoice invoice, DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(invoice);

        decimal outstanding = invoice.OutstandingAmount;
        var stage = StageFor(invoice, outstanding, today);

        var milestones = new List<DunningMilestone>
        {
            new("Due", invoice.DueDate, today >= invoice.DueDate),
            new("Reminder", invoice.ReminderDate, today >= invoice.ReminderDate),
            new("Suspension", invoice.SuspensionDate, today >= invoice.SuspensionDate)
        };

        return new InvoiceDunningProjection
        {
            InvoiceId = invoice.Id,
            InvoiceNumber = invoice.InvoiceNumber,
            PolicyId = invoice.PolicyId,
            DueDate = invoice.DueDate,
            ReminderDate = invoice.ReminderDate,
            SuspensionDate = invoice.SuspensionDate,
            Outstanding = outstanding,
            Stage = stage,
            DaysPastDue = Math.Max(0, today.DayNumber - invoice.DueDate.DayNumber),
            Milestones = milestones
        };
    }

    public DunningOverview Summarize(IEnumerable<Invoice> invoices, DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(invoices);

        var projections = invoices
            .Where(i => !i.IsCreditNote && i.Status != InvoiceStatus.Cancelled)
            .Select(i => Project(i, today))
            .Where(p => p.Stage != DunningStage.Settled)
            .ToList();

        return new DunningOverview
        {
            AsOf = today,
            OpenInvoiceCount = projections.Count,
            TotalOutstanding = projections.Sum(p => p.Outstanding),
            ByStage = projections
                .GroupBy(p => p.Stage)
                .OrderBy(g => g.Key)
                .ToDictionary(g => g.Key, g => g.Count()),
            OutstandingByStage = projections
                .GroupBy(p => p.Stage)
                .OrderBy(g => g.Key)
                .ToDictionary(g => g.Key, g => g.Sum(p => p.Outstanding))
        };
    }

    public IReadOnlyList<InstallmentProjection> ProjectSchedule(Policy policy)
    {
        ArgumentNullException.ThrowIfNull(policy);

        var schedule = _installments.Build(policy.AnnualPremium, policy.PaymentFrequency, policy.StartDate);

        return schedule
            .Where(i => i.DueDate < policy.EndDate && IsBeforeCancellation(policy, i.DueDate))
            .Select(i => ToProjection(i, schedule.Count))
            .ToList();
    }

    public IReadOnlyList<InstallmentProjection> RemainingSchedule(Policy policy, DateOnly today) =>
        ProjectSchedule(policy).Where(p => p.DueDate >= today).ToList();

    private static InstallmentProjection ToProjection(Installment installment, int count)
    {
        var issued = installment.DueDate;
        var due = Invoice.DueDateFor(issued);

        return new InstallmentProjection(
            installment.Number,
            count,
            issued,
            due,
            due.AddDays(15),
            due.AddDays(45),
            installment.Amount);
    }

    private static bool IsBeforeCancellation(Policy policy, DateOnly date) =>
        policy.CancelledOn is not { } cancelledOn || date < cancelledOn;

    private static DunningStage StageFor(Invoice invoice, decimal outstanding, DateOnly today)
    {
        if (invoice.IsCreditNote || invoice.Status is InvoiceStatus.Paid or InvoiceStatus.Cancelled || outstanding <= 0m)
        {
            return DunningStage.Settled;
        }

        if (today >= invoice.SuspensionDate)
        {
            return DunningStage.SuspensionDue;
        }

        if (today >= invoice.ReminderDate)
        {
            return DunningStage.ReminderDue;
        }

        return today > invoice.DueDate ? DunningStage.Due : DunningStage.NotYetDue;
    }
}