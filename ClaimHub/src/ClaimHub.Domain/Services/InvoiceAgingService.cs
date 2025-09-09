using ClaimHub.Domain.Entities;
using ClaimHub.Domain.Enums;
using ClaimHub.Domain.Models;

namespace ClaimHub.Domain.Services;

/// <summary>
/// Groups open invoices by how long they have been past their due date, using the invoice's own outstanding amount.
/// </summary>
public class InvoiceAgingService
{
    public const int DefaultDebtorCount = 5;

    public AgingBucket BucketFor(Invoice invoice, DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(invoice);

        int daysPastDue = today.DayNumber - invoice.DueDate.DayNumber;
        return daysPastDue switch
        {
            <= 0 => AgingBucket.NotYetDue,
            <= 30 => AgingBucket.Days1To30,
            <= 60 => AgingBucket.Days31To60,
            <= 90 => AgingBucket.Days61To90,
            _ => AgingBucket.Over90
        };
    }

    public bool IsOpen(Invoice invoice) =>
        invoice.Status is InvoiceStatus.Issued or InvoiceStatus.Overdue && invoice.OutstandingAmount > 0m;

    public ReceivablesAging Build(IEnumerable<Invoice> invoices, DateOnly today, int debtorCount = DefaultDebtorCount)
    {
        ArgumentNullException.ThrowIfNull(invoices);
        ArgumentOutOfRangeException.ThrowIfLessThan(debtorCount, 0);

        var open = invoices.Where(IsOpen).ToList();

        var buckets = Enum.GetValues<AgingBucket>()
            .Select(bucket =>
            {
                var inBucket = open.Where(i => BucketFor(i, today) == bucket).ToList();
                return new AgingBucketTotal(bucket, inBucket.Count, inBucket.Sum(i => i.OutstandingAmount));
            })
            .ToList();

        return new ReceivablesAging
        {
            AsOf = today,
            Buckets = buckets,
            TopDebtors = TopDebtors(open, today, debtorCount)
        };
    }

    public IReadOnlyList<DebtorLine> TopDebtors(IEnumerable<Invoice> openInvoices, DateOnly today, int count) =>
        openInvoices
            .Where(i => BucketFor(i, today) != AgingBucket.NotYetDue)
            .GroupBy(i => i.PolicyId)
            .Select(g => new DebtorLine(
                g.Key,
                g.First().Policy?.PolicyNumber ?? string.Empty,
                g.Count(),
                g.Sum(i => i.OutstandingAmount),
                g.Max(i => today.DayNumber - i.DueDate.DayNumber)))
            .OrderByDescending(d => d.Outstanding)
            .ThenBy(d => d.PolicyId)
            .Take(count)
            .ToList();
}