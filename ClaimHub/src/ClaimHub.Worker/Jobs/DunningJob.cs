using ClaimHub.Domain.Entities;
using ClaimHub.Domain.Enums;
using ClaimHub.Infrastructure.Data;
using ClaimHub.Infrastructure.External;
using ClaimHub.Worker.Options;
using ClaimHub.Worker.Planning;
using ClaimHub.Worker.Scheduling;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ClaimHub.Worker.Jobs;

/// <summary>
/// Follows up on unpaid invoices: marks them overdue, sends reminders and suspends the policy
/// when the invoice stays open for too long.
/// </summary>
public sealed class DunningJob : ScheduledJob
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IEmailSender _email;
    private readonly DunningPlanner _planner;
    private readonly DunningJobOptions _options;
    private readonly ILogger<DunningJob> _logger;

    public DunningJob(
        IServiceScopeFactory scopeFactory,
        IEmailSender email,
        DunningPlanner planner,
        IOptions<DunningJobOptions> options,
        IJobClock clock,
        ILogger<DunningJob> logger)
        : base("DunningJob", options.Value, clock, logger)
    {
        _scopeFactory = scopeFactory;
        _email = email;
        _planner = planner;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task<JobRunResult> RunAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ClaimHubDbContext>();

        var today = Clock.Today;
        var now = Clock.UtcNow;

        var invoices = await db.Invoices
            .Include(i => i.Payments)
            .Include(i => i.Policy).ThenInclude(p => p!.PolicyHolder)
            .Where(i => (i.Status == InvoiceStatus.Issued || i.Status == InvoiceStatus.Overdue)
                && i.Amount > 0m
                && i.DueDate < today)
            .OrderBy(i => i.DueDate)
            .ThenBy(i => i.Id)
            .Take(_options.MaxInvoicesPerRun)
            .ToListAsync(cancellationToken);

        var tally = new DunningTally();

        foreach (var invoice in invoices)
        {
            var decision = _planner.Decide(invoice, today, _options.ReminderAfterDays);
            tally.Record(decision);
            if (decision.Skip)
            {
                continue;
            }

            var policy = invoice.Policy!;
            var holder = policy.PolicyHolder!;

            if (decision.MarkOverdue)
            {
                invoice.Status = InvoiceStatus.Overdue;
            }

            if (decision.SendReminder)
            {
                await _email.SendAsync(DunningMessages.Reminder(holder, policy, invoice), cancellationToken);
                invoice.ReminderSentAt = now;
            }

            if (decision.Suspend)
            {
                policy.Suspend();
                policy.UpdatedAt = now;

                await _email.SendAsync(DunningMessages.Suspension(holder, policy, invoice), cancellationToken);
                db.AuditEntries.Add(AuditEntry.For(
                    nameof(Policy),
                    policy.Id,
                    AuditAction.StatusChanged,
                    "dunning-job",
                    now,
                    $"Suspended for unpaid invoice {invoice.InvoiceNumber}, due {invoice.DueDate:yyyy-MM-dd}"));

                _logger.LogWarning(
                    "Policy {PolicyNumber} suspended: invoice {InvoiceNumber} is {Days} days past due (agreed rule: {Rule} days).",
                    policy.PolicyNumber, invoice.InvoiceNumber, decision.DaysPastDue, _options.SuspensionAfterDays);
            }
        }

        await db.SaveChangesAsync(cancellationToken);

        return tally.ToResult(invoices.Count);
    }
}