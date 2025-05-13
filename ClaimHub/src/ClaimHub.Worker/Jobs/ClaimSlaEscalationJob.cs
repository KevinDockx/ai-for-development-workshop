using ClaimHub.Domain.Common;
using ClaimHub.Domain.Entities;
using ClaimHub.Domain.Enums;
using ClaimHub.Domain.Rules;
using ClaimHub.Infrastructure.Data;
using ClaimHub.Worker.Options;
using ClaimHub.Worker.Planning;
using ClaimHub.Worker.Scheduling;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ClaimHub.Worker.Jobs;

/// <summary>
/// Escalates claims that nobody has assessed within the SLA to a senior claims handler.
/// </summary>
public sealed class ClaimSlaEscalationJob : ScheduledJob
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ClaimSlaService _slaService;
    private readonly SlaEscalationPlanner _planner;
    private readonly ClaimSlaEscalationJobOptions _options;
    private readonly ILogger<ClaimSlaEscalationJob> _logger;

    public ClaimSlaEscalationJob(
        IServiceScopeFactory scopeFactory,
        ClaimSlaService slaService,
        SlaEscalationPlanner planner,
        IOptions<ClaimSlaEscalationJobOptions> options,
        IJobClock clock,
        ILogger<ClaimSlaEscalationJob> logger)
        : base("ClaimSlaEscalationJob", options.Value, clock, logger)
    {
        _scopeFactory = scopeFactory;
        _slaService = slaService;
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

        var openClaims = await db.Claims
            .Where(c => !c.IsDeleted
                && (c.Status == ClaimStatus.Reported || c.Status == ClaimStatus.UnderAssessment)
                && c.FirstAssessmentAt == null
                && c.EscalatedAt == null)
            .OrderBy(c => c.ReportedAt)
            .ToListAsync(cancellationToken);

        var overdue = _planner.SelectOverdue(openClaims, today, _options.EscalationAfterDays);

        if (overdue.Count == 0)
        {
            return _planner.NothingOverdue(openClaims.Count);
        }

        var seniors = await db.Users
            .Where(u => u.IsActive && u.Role == UserRole.SeniorClaimsHandler)
            .OrderBy(u => u.Id)
            .ToListAsync(cancellationToken);

        if (seniors.Count == 0)
        {
            _logger.LogWarning("{Count} claims are overdue for assessment but no senior claims handler is available.", overdue.Count);
            return _planner.NoSeniorAvailable(openClaims.Count);
        }

        var workload = await LoadWorkloadAsync(db, seniors, cancellationToken);
        var assignments = _planner.Assign(overdue, seniors, workload, today);

        foreach (var (claim, senior, daysOpen) in assignments)
        {
            _slaService.Escalate(claim, senior, now);

            db.AuditEntries.Add(AuditEntry.For(
                nameof(Claim),
                claim.Id,
                AuditAction.Updated,
                "sla-escalation-job",
                now,
                $"Escalated to {senior.Email} after {daysOpen} calendar days without assessment"));

            _logger.LogInformation(
                "Claim {ClaimNumber} escalated to {Handler} after {Days} calendar days.",
                claim.ClaimNumber, senior.DisplayName, daysOpen);
        }

        await db.SaveChangesAsync(cancellationToken);

        return _planner.Escalated(openClaims.Count, assignments.Count, seniors.Count, _options.EscalationAfterDays);
    }

    private static async Task<Dictionary<int, int>> LoadWorkloadAsync(
        ClaimHubDbContext db,
        IReadOnlyList<ApplicationUser> seniors,
        CancellationToken cancellationToken)
    {
        var ids = seniors.Select(s => s.Id).ToList();
        var counts = await db.Claims
            .Where(c => !c.IsDeleted
                && c.AssignedHandlerId != null
                && ids.Contains(c.AssignedHandlerId.Value)
                && c.Status != ClaimStatus.Closed
                && c.Status != ClaimStatus.Withdrawn
                && c.Status != ClaimStatus.Rejected)
            .GroupBy(c => c.AssignedHandlerId!.Value)
            .Select(g => new { HandlerId = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var workload = ids.ToDictionary(id => id, _ => 0);
        foreach (var entry in counts)
        {
            workload[entry.HandlerId] = entry.Count;
        }

        return workload;
    }
}