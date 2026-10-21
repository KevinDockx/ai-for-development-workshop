using System.Diagnostics;
using ClaimHub.Worker.Options;

namespace ClaimHub.Worker.Scheduling;

/// <summary>
/// Base class for the jobs that run on a fixed interval. The next run is only awaited after the
/// current one has finished, so a slow run never overlaps with the following tick.
/// </summary>
public abstract class ScheduledJob : BackgroundService
{
    private readonly string _name;
    private readonly JobScheduleOptions _schedule;
    private readonly ILogger _logger;
    private int _runNumber;

    protected ScheduledJob(string name, JobScheduleOptions schedule, IJobClock clock, ILogger logger)
    {
        _name = name;
        _schedule = schedule;
        _logger = logger;
        Clock = clock;
    }

    protected IJobClock Clock { get; }

    protected abstract Task<JobRunResult> RunAsync(CancellationToken cancellationToken);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_schedule.Enabled)
        {
            _logger.LogInformation("{Job} is disabled and will not run.", _name);
            return;
        }

        _logger.LogInformation(
            "{Job} scheduled: first run in {Delay}, interval {Interval}, run once {RunOnce}.",
            _name, _schedule.InitialDelay, _schedule.Interval, _schedule.RunOnce);

        try
        {
            await Task.Delay(_schedule.InitialDelay, stoppingToken);

            using var timer = new PeriodicTimer(_schedule.Interval);
            do
            {
                await ExecuteRunAsync(stoppingToken);
            }
            while (!_schedule.RunOnce && await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("{Job} stopped.", _name);
            return;
        }

        _logger.LogInformation("{Job} finished its scheduled runs.", _name);
    }

    private async Task ExecuteRunAsync(CancellationToken cancellationToken)
    {
        int run = ++_runNumber;
        _logger.LogInformation("{Job} run {Run} started for {Today:yyyy-MM-dd}.", _name, run, Clock.Today);

        var stopwatch = Stopwatch.StartNew();
        var result = await RunAsync(cancellationToken);
        stopwatch.Stop();

        _logger.LogInformation(
            "{Job} run {Run} completed in {Elapsed} ms: {Result}.",
            _name, run, stopwatch.ElapsedMilliseconds, result);
    }
}