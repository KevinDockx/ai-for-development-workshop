namespace ClaimHub.Domain.Services;

public class ClaimNumberGenerator
{
    private readonly IClaimNumberSequence _sequence;

    public ClaimNumberGenerator(IClaimNumberSequence sequence)
    {
        _sequence = sequence;
    }

    public async Task<string> GenerateAsync(int year, CancellationToken cancellationToken = default)
    {
        int next = await _sequence.NextAsync(year, cancellationToken);
        return Format(year, next);
    }

    public static string Format(int year, int sequence)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(sequence, 1);
        return $"CLM-{year}-{sequence:D6}";
    }

    public static bool IsWellFormed(string? claimNumber)
    {
        var parts = claimNumber?.Split('-');
        return parts is { Length: 3 }
            && parts[0] == "CLM"
            && int.TryParse(parts[1], out _)
            && parts[2].Length == 6
            && int.TryParse(parts[2], out _);
    }
}
