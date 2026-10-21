using ClaimHub.Domain.Entities;
using ClaimHub.Domain.Enums;
using ClaimHub.Domain.Models;
using ClaimHub.Domain.Services;
using ClaimHub.Infrastructure.Data;
using ClaimHub.Worker.Options;
using ClaimHub.Worker.Planning;
using ClaimHub.Worker.Scheduling;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ClaimHub.Worker.Jobs;

/// <summary>
/// Creates the installment invoices that fall due in the coming weeks for every active policy.
/// </summary>
public sealed class BillingRunJob : ScheduledJob
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly BillingRunJobOptions _options;
    private readonly BillingRunPlanner _planner;
    private readonly IInvoiceNumberSequence _invoiceNumbers;
    private readonly ILogger<BillingRunJob> _logger;

    public BillingRunJob(
        IServiceScopeFactory scopeFactory,
        IOptions<BillingRunJobOptions> options,
        BillingRunPlanner planner,
        IInvoiceNumberSequence invoiceNumbers,
        IJobClock clock,
        ILogger<BillingRunJob> logger)
        : base("BillingRunJob", options.Value, clock, logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _planner = planner;
        _invoiceNumbers = invoiceNumbers;
        _logger = logger;
    }

    protected override async Task<JobRunResult> RunAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ClaimHubDbContext>();

        var today = Clock.Today;
        var horizon = today.AddDays(_options.LookaheadDays);

        var policies = await db.Policies
            .Where(p => !p.IsDeleted
                && p.Status == PolicyStatus.Active
                && p.StartDate <= horizon
                && p.EndDate > today)
            .OrderBy(p => p.Id)
            .Take(_options.MaxPoliciesPerRun)
            .ToListAsync(cancellationToken);

        var plan = _planner.Plan(policies, today, horizon);
        if (plan.IsEmpty)
        {
            return plan.EmptyResult();
        }

        var pending = plan.Pending;
        var numbers = new List<string>(pending.Count);
        decimal total = 0m;

        for (int i = 0; i < pending.Count; i++)
        {
            var item = pending[i];
            var invoice = Invoice.ForInstallment(item.Policy, item.Installment, today);
            invoice.InvoiceNumber = await _invoiceNumbers.NextAsync(InvoiceNumberKinds.Invoice, today.Year, cancellationToken);
            numbers.Add(invoice.InvoiceNumber);
            invoice.InstallmentCount = item.InstallmentCount;

            db.Invoices.Add(invoice);
            total += invoice.Amount;

            _logger.LogDebug(
                "Invoice {InvoiceNumber} for policy {PolicyNumber}: installment {Number} of {Count}, {Amount} EUR, due {DueDate:yyyy-MM-dd}.",
                invoice.InvoiceNumber, item.Policy.PolicyNumber, item.Installment.Number, item.InstallmentCount, invoice.Amount, invoice.DueDate);
        }

        await db.SaveChangesAsync(cancellationToken);

        return plan.Result(total, numbers);
    }
}