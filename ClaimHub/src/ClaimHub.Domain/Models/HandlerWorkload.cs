namespace ClaimHub.Domain.Models;

public sealed record HandlerWorkloadLine(
    int HandlerId,
    string DisplayName,
    bool IsSenior,
    int OpenClaims,
    int EscalatedClaims,
    decimal AssessedExposure,
    int OldestOpenDays)
{
    public bool IsOverloaded(int threshold) => OpenClaims > threshold;
}

public sealed class HandlerWorkloadReport
{
    public DateOnly AsOf { get; init; }

    public IReadOnlyList<HandlerWorkloadLine> Handlers { get; init; } = Array.Empty<HandlerWorkloadLine>();

    public int UnassignedOpenClaims { get; init; }

    public int OldestUnassignedDays { get; init; }

    public int TotalOpenClaims => Handlers.Sum(h => h.OpenClaims) + UnassignedOpenClaims;

    public HandlerWorkloadLine? Busiest => Handlers
        .OrderByDescending(h => h.OpenClaims)
        .ThenBy(h => h.HandlerId)
        .FirstOrDefault();

    public HandlerWorkloadLine? LeastLoadedSenior => Handlers
        .Where(h => h.IsSenior)
        .OrderBy(h => h.OpenClaims)
        .ThenBy(h => h.HandlerId)
        .FirstOrDefault();
}