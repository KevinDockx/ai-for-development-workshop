using ClaimHub.Contracts.Policies;
using ClaimHub.Api.Policies;
using ClaimHub.Domain.Common;
using ClaimHub.Domain.Enums;
using ClaimHub.Infrastructure.Data;
using ClaimHub.Worker.Options;
using ClaimHub.Worker.Planning;
using ClaimHub.Worker.Scheduling;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ClaimHub.Worker.Jobs;

/// <summary>
/// Renews active policies that are about to end by calling the same <see cref="PolicyService"/>
/// the API uses for manual renewals.
/// </summary>
public sealed class RenewalJob : ScheduledJob
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RenewalJobOptions _options;
    private readonly ILogger<RenewalJob> _logger;
    private readonly RenewalPlanner _planner;

    public RenewalJob(
        IServiceScopeFactory scopeFactory,
        IOptions<RenewalJobOptions> options,
        RenewalPlanner planner,
        IJobClock clock,
        ILogger<RenewalJob> logger)
        : base("RenewalJob", options.Value, clock, logger)
    {
        _planner = planner;
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task<JobRunResult> RunAsync(CancellationToken cancellationToken)
    {
        var candidates = await FindCandidatesAsync(cancellationToken);

        var tally = _planner.NewTally();

        foreach (var candidate in candidates)
        {
            // A fresh scope per policy keeps one failed renewal from poisoning the next one.
            using var scope = _scopeFactory.CreateScope();
            var policies = scope.ServiceProvider.GetRequiredService<PolicyService>();

            try
            {
                var result = await policies.RenewAsync(
                    candidate.Id,
                    new RenewPolicyRequest(),
                    _options.RunAsUser,
                    cancellationToken);

                if (result == null)
                {
                    tally.Reject(candidate.PolicyNumber, "Renewal returned no policy");
                    continue;
                }

                tally.Renew(candidate.PolicyNumber, result.PolicyNumber);
                _logger.LogInformation(
                    "Policy {PolicyNumber} (ending {EndDate:yyyy-MM-dd}) renewed as {NewPolicyNumber}.",
                    candidate.PolicyNumber, candidate.EndDate, result.PolicyNumber);
            }
            catch (DomainRuleException ex)
            {
                tally.Reject(candidate.PolicyNumber, ex.Message);
                _logger.LogWarning("Policy {PolicyNumber} was not renewed: {Reason}", candidate.PolicyNumber, ex.Message);
            }
        }

        foreach (var line in _planner.SummarizeRejections(tally))
        {
            _logger.LogInformation("Renewal rejections: {Summary}", line);
        }

        return _planner.ToResult(candidates.Count, tally);
    }

    private async Task<List<RenewalCandidate>> FindCandidatesAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ClaimHubDbContext>();

        var today = Clock.Today;
        var windowEnd = today.AddDays(_options.RenewalWindowDays);

        return await db.Policies
            .AsNoTracking()
            .Where(p => !p.IsDeleted
                && p.Status == PolicyStatus.Active
                && p.EndDate >= today
                && p.EndDate <= windowEnd
                && !db.Policies.Any(r => r.RenewedFromPolicyId == p.Id))
            .OrderBy(p => p.EndDate)
            .ThenBy(p => p.Id)
            .Take(_options.MaxRenewalsPerRun)
            .Select(p => new RenewalCandidate(p.Id, p.PolicyNumber, p.EndDate))
            .ToListAsync(cancellationToken);
    }

    private sealed record RenewalCandidate(int Id, string PolicyNumber, DateOnly EndDate);
}