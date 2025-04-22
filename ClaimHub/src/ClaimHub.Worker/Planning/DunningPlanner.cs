using ClaimHub.Domain.Entities;
using ClaimHub.Domain.Enums;
using ClaimHub.Worker.Scheduling;

namespace ClaimHub.Worker.Planning;

public sealed record DunningDecision(bool Skip, bool MarkOverdue, bool SendReminder, bool Suspend, int DaysPastDue)
{
    public static DunningDecision Skipped { get; } = new(true, false, false, false, 0);

    public bool HasWork => MarkOverdue || SendReminder || Suspend;
}

public sealed class DunningTally
{
    public int MarkedOverdue { get; private set; }

    public int Reminders { get; private set; }

    public int Suspended { get; private set; }

    public int Skipped { get; private set; }

    public int Changed => MarkedOverdue + Reminders + Suspended;

    public void Record(DunningDecision decision)
    {
        if (decision.Skip)
        {
            Skipped++;
            return;
        }

        MarkedOverdue += decision.MarkOverdue ? 1 : 0;
        Reminders += decision.SendReminder ? 1 : 0;
        Suspended += decision.Suspend ? 1 : 0;
    }

    public JobRunResult ToResult(int examined) =>
        new(examined, Changed, Skipped, $"marked overdue {MarkedOverdue}, reminders {Reminders}, policies suspended {Suspended}");
}

/// <summary>Decides what the dunning run should do with one overdue invoice, using the grace period the job has always used.</summary>
public sealed class DunningPlanner
{
    public const int SuspensionGraceDays = 30;

    public DunningDecision Decide(Invoice invoice, DateOnly today, int reminderAfterDays)
    {
        var policy = invoice.Policy;
        if (policy == null || policy.PolicyHolder == null || policy.IsDeleted || invoice.OutstandingAmount <= 0m)
        {
            return DunningDecision.Skipped;
        }

        return new DunningDecision(
            Skip: false,
            MarkOverdue: invoice.Status == InvoiceStatus.Issued,
            SendReminder: invoice.ReminderSentAt is null && today >= invoice.DueDate.AddDays(reminderAfterDays),
            Suspend: policy.Status == PolicyStatus.Active && today >= invoice.DueDate.AddDays(SuspensionGraceDays),
            DaysPastDue: today.DayNumber - invoice.DueDate.DayNumber);
    }
}