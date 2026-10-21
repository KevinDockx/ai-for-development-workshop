using ClaimHub.Worker.Export;
using ClaimHub.Worker.Options;
using ClaimHub.Worker.Scheduling;
using Microsoft.Extensions.Options;

namespace ClaimHub.Worker.Jobs;

/// <summary>
/// Writes the nightly JSON export. This job predates the shared scheduler base class and keeps
/// its own <see cref="Timer"/>: each run re-arms the timer when it is done, so exports never overlap.
/// </summary>
public sealed class NightlyExportJob : BackgroundService
{
    private const string JobName = "NightlyExportJob";

    private readonly NightlyExportJobOptions _options;
    private readonly NightlyExportWriter _writer;
    private readonly IJobClock _clock;
    private readonly ILogger<NightlyExportJob> _logger;
    private readonly object _gate = new();

    private Timer? _timer;
    private bool _stopping;
    private int _runNumber;

    public NightlyExportJob(
        IServiceScopeFactory scopeFactory,
        IOptions<NightlyExportJobOptions> options,
        IJobClock clock,
        ILogger<NightlyExportJob> logger)
    {
        _options = options.Value;
        _writer = new NightlyExportWriter(scopeFactory, _options.ExportFolder, _options.FilePrefix);
        _clock = clock;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("{Job} is disabled and will not run.", JobName);
            return;
        }

        _logger.LogInformation(
            "{Job} scheduled: first run in {Delay}, interval {Interval}, run once {RunOnce}.",
            JobName, _options.InitialDelay, _options.Interval, _options.RunOnce);

        lock (_gate)
        {
            _timer = new Timer(OnTimerElapsed, null, _options.InitialDelay, Timeout.InfiniteTimeSpan);
        }

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("{Job} stopped.", JobName);
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            _stopping = true;
            _timer?.Dispose();
        }

        return base.StopAsync(cancellationToken);
    }

    private async void OnTimerElapsed(object? state)
    {
        int run = Interlocked.Increment(ref _runNumber);
        _logger.LogInformation("{Job} run {Run} started for {Today:yyyy-MM-dd}.", JobName, run, _clock.Today);

        var started = DateTimeOffset.UtcNow;
        var result = await _writer.WriteAsync(_clock.UtcNow);
        var elapsed = DateTimeOffset.UtcNow - started;

        _logger.LogInformation(
            "{Job} run {Run} completed in {Elapsed} ms: wrote {Path} ({Bytes} bytes) with {Holders} policyholders, {Policies} policies, {Claims} claims.",
            JobName, run, (long)elapsed.TotalMilliseconds, result.Path, result.Bytes, result.PolicyHolders, result.Policies, result.Claims);

        ScheduleNextRun();
    }

    private void ScheduleNextRun()
    {
        if (_options.RunOnce)
        {
            _logger.LogInformation("{Job} finished its scheduled runs.", JobName);
            return;
        }

        lock (_gate)
        {
            if (!_stopping)
            {
                _timer?.Change(_options.Interval, Timeout.InfiniteTimeSpan);
            }
        }
    }
}