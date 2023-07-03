using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ClaimHub.Infrastructure.Data;

public class DateTimeOffsetTicksConverter : ValueConverter<DateTimeOffset, long>
{
    public DateTimeOffsetTicksConverter()
        : base(value => value.UtcTicks, ticks => new DateTimeOffset(ticks, TimeSpan.Zero))
    {
    }
}

public class NullableDateTimeOffsetTicksConverter : ValueConverter<DateTimeOffset?, long?>
{
    public NullableDateTimeOffsetTicksConverter()
        : base(
            value => value.HasValue ? value.Value.UtcTicks : (long?)null,
            ticks => ticks.HasValue ? new DateTimeOffset(ticks.Value, TimeSpan.Zero) : (DateTimeOffset?)null)
    {
    }
}
