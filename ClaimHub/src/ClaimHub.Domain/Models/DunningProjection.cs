namespace ClaimHub.Domain.Models;

public enum DunningStage
{
    Settled = 0,
    NotYetDue = 1,
    Due = 2,
    ReminderDue = 3,
    SuspensionDue = 4
}

public sealed record DunningMilestone(string Name, DateOnly Date, bool Reached);

public sealed class InvoiceDunningProjection
{
    public int InvoiceId { get; init; }

    public string InvoiceNumber { get; init; } = string.Empty;

    public int PolicyId { get; init; }

    public DateOnly DueDate { get; init; }

    public DateOnly ReminderDate { get; init; }

    public DateOnly SuspensionDate { get; init; }

    public decimal Outstanding { get; init; }

    public DunningStage Stage { get; init; }

    public int DaysPastDue { get; init; }

    public IReadOnlyList<DunningMilestone> Milestones { get; init; } = Array.Empty<DunningMilestone>();

    public DunningMilestone? NextMilestone => Milestones.FirstOrDefault(m => !m.Reached);
}

public sealed record InstallmentProjection(
    int Number,
    int Count,
    DateOnly IssueDate,
    DateOnly DueDate,
    DateOnly ReminderDate,
    DateOnly SuspensionDate,
    decimal Amount);

public sealed class DunningOverview
{
    public DateOnly AsOf { get; init; }

    public int OpenInvoiceCount { get; init; }

    public decimal TotalOutstanding { get; init; }

    public IReadOnlyDictionary<DunningStage, int> ByStage { get; init; } = new Dictionary<DunningStage, int>();

    public IReadOnlyDictionary<DunningStage, decimal> OutstandingByStage { get; init; } = new Dictionary<DunningStage, decimal>();

    public int CountFor(DunningStage stage) => ByStage.TryGetValue(stage, out int count) ? count : 0;

    public decimal OutstandingFor(DunningStage stage) => OutstandingByStage.TryGetValue(stage, out decimal amount) ? amount : 0m;

    public int PastDueCount => CountFor(DunningStage.Due) + CountFor(DunningStage.ReminderDue) + CountFor(DunningStage.SuspensionDue);
}