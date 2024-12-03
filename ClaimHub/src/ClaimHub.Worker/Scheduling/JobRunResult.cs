namespace ClaimHub.Worker.Scheduling;

/// <summary>What a single job run looked at and what it changed, for the completion log line.</summary>
public sealed record JobRunResult(int Examined, int Changed, int Skipped = 0, string? Detail = null)
{
    public override string ToString()
    {
        string text = $"examined {Examined}, changed {Changed}, skipped {Skipped}";
        return string.IsNullOrEmpty(Detail) ? text : $"{text} ({Detail})";
    }
}