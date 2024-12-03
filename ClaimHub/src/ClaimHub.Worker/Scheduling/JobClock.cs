using ClaimHub.Domain.Common;
using ClaimHub.Worker.Options;
using Microsoft.Extensions.Options;

namespace ClaimHub.Worker.Scheduling;

public interface IJobClock
{
    DateOnly Today { get; }

    DateTimeOffset UtcNow { get; }
}

public sealed class JobClock : IJobClock
{
    private static readonly TimeOnly AnchoredTimeOfDay = new(10, 0);

    private readonly DateOnly? _asOfDate;

    public JobClock(IOptions<JobClockOptions> options)
    {
        _asOfDate = options.Value.AsOfDate;
    }

    public DateTimeOffset UtcNow => _asOfDate is { } asOf
        ? new DateTimeOffset(asOf.ToDateTime(AnchoredTimeOfDay), TimeSpan.Zero)
        : DateTimeOffset.UtcNow;

    public DateOnly Today => BelgianTime.ToLocalDate(UtcNow);
}