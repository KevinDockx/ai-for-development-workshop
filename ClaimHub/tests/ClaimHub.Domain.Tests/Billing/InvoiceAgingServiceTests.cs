using ClaimHub.Domain.Entities;
using ClaimHub.Domain.Enums;
using ClaimHub.Domain.Models;
using ClaimHub.Domain.Services;
using Xunit;

namespace ClaimHub.Domain.Tests.Billing;

public class InvoiceAgingServiceTests
{
    private static readonly DateOnly Today = new(2025, 6, 30);
    private readonly InvoiceAgingService _service = new();

    private static Invoice Open(int policyId, decimal amount, DateOnly due, InvoiceStatus status = InvoiceStatus.Issued) =>
        new() { PolicyId = policyId, Amount = amount, DueDate = due, Status = status };

    [Theory]
    [InlineData(0, AgingBucket.NotYetDue)]
    [InlineData(-5, AgingBucket.NotYetDue)]
    [InlineData(30, AgingBucket.Days1To30)]
    [InlineData(31, AgingBucket.Days31To60)]
    [InlineData(61, AgingBucket.Days61To90)]
    [InlineData(120, AgingBucket.Over90)]
    public void BucketFor_uses_days_past_due(int daysPastDue, AgingBucket expected)
    {
        var invoice = Open(1, 100m, Today.AddDays(-daysPastDue));

        Assert.Equal(expected, _service.BucketFor(invoice, Today));
    }

    [Fact]
    public void Build_ignores_paid_and_credit_note_invoices()
    {
        var invoices = new[]
        {
            Open(1, 100m, Today.AddDays(-10)),
            Open(1, 50m, Today.AddDays(-10), InvoiceStatus.Paid),
            Open(2, -40m, Today.AddDays(-10), InvoiceStatus.CreditNote)
        };

        var aging = _service.Build(invoices, Today);

        Assert.Equal(1, aging.OpenInvoiceCount);
        Assert.Equal(100m, aging.TotalOutstanding);
    }

    [Fact]
    public void Build_orders_debtors_by_outstanding_amount()
    {
        var invoices = new[]
        {
            Open(1, 100m, Today.AddDays(-40)),
            Open(2, 300m, Today.AddDays(-5)),
            Open(3, 200m, Today.AddDays(10))
        };

        var aging = _service.Build(invoices, Today);

        Assert.Equal(new[] { 2, 1 }, aging.TopDebtors.Select(d => d.PolicyId));
        Assert.Equal(40, aging.TopDebtors[1].OldestDaysPastDue);
    }
}