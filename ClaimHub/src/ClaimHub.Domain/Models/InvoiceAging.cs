namespace ClaimHub.Domain.Models;

public enum AgingBucket
{
    NotYetDue = 0,
    Days1To30 = 1,
    Days31To60 = 2,
    Days61To90 = 3,
    Over90 = 4
}

public sealed record AgingBucketTotal(AgingBucket Bucket, int InvoiceCount, decimal Outstanding);

public sealed record DebtorLine(int PolicyId, string PolicyNumber, int InvoiceCount, decimal Outstanding, int OldestDaysPastDue);

public sealed class ReceivablesAging
{
    public DateOnly AsOf { get; init; }

    public IReadOnlyList<AgingBucketTotal> Buckets { get; init; } = Array.Empty<AgingBucketTotal>();

    public IReadOnlyList<DebtorLine> TopDebtors { get; init; } = Array.Empty<DebtorLine>();

    public decimal TotalOutstanding => Buckets.Sum(b => b.Outstanding);

    public int OpenInvoiceCount => Buckets.Sum(b => b.InvoiceCount);

    public decimal PastDueOutstanding => Buckets.Where(b => b.Bucket != AgingBucket.NotYetDue).Sum(b => b.Outstanding);

    public decimal PastDueShare => TotalOutstanding == 0m
        ? 0m
        : Math.Round(PastDueOutstanding * 100m / TotalOutstanding, 1, MidpointRounding.AwayFromZero);

    public AgingBucketTotal For(AgingBucket bucket) =>
        Buckets.FirstOrDefault(b => b.Bucket == bucket) ?? new AgingBucketTotal(bucket, 0, 0m);
}