namespace ClaimHub.Domain.Common;

public static class BelgianTime
{
    private static readonly TimeZoneInfo Zone = Resolve();

    public static DateOnly ToLocalDate(DateTimeOffset timestamp)
    {
        var local = TimeZoneInfo.ConvertTime(timestamp, Zone);
        return DateOnly.FromDateTime(local.DateTime);
    }

    public static DateOnly Today(TimeProvider? clock = null) =>
        ToLocalDate((clock ?? TimeProvider.System).GetUtcNow());

    private static TimeZoneInfo Resolve()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Europe/Brussels");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Romance Standard Time");
        }
    }
}
