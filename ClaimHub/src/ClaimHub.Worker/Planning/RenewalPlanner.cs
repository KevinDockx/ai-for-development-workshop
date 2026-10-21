using ClaimHub.Worker.Scheduling;

namespace ClaimHub.Worker.Planning;

public sealed record RenewalOutcome(string OldPolicyNumber, string? NewPolicyNumber, string? RejectionReason)
{
    public bool Renewed => NewPolicyNumber != null;
}

/// <summary>Collects the per-policy outcomes of a renewal run and turns them into the run result.</summary>
public sealed class RenewalTally
{
    private readonly List<RenewalOutcome> _outcomes = new();

    public int Processed => _outcomes.Count;
    public int Renewed => _outcomes.Count(o => o.Renewed);
    public int Rejected => _outcomes.Count(o => !o.Renewed);

    public IReadOnlyList<RenewalOutcome> Outcomes => _outcomes;

    public void Renew(string oldNumber, string newNumber) =>
        _outcomes.Add(new RenewalOutcome(oldNumber, newNumber, null));

    public void Reject(string oldNumber, string reason) =>
        _outcomes.Add(new RenewalOutcome(oldNumber, null, reason));
}

public sealed class RenewalPlanner
{
    public RenewalTally NewTally() => new();

    public JobRunResult ToResult(int candidateCount, RenewalTally tally)
    {
        var renewedNumbers = tally.Outcomes
            .Where(o => o.Renewed)
            .Select(o => $"{o.OldPolicyNumber}->{o.NewPolicyNumber}")
            .ToList();

        string detail = renewedNumbers.Count == 0 ? "nothing renewed" : string.Join(", ", renewedNumbers);
        return new JobRunResult(candidateCount, tally.Renewed, tally.Rejected, detail);
    }

    /// <summary>Groups the rejection reasons so a run with many failures stays readable in the log.</summary>
    public IReadOnlyList<string> SummarizeRejections(RenewalTally tally)
    {
        return tally.Outcomes
            .Where(o => !o.Renewed)
            .GroupBy(o => o.RejectionReason ?? "Rejected without a reason")
            .OrderByDescending(g => g.Count())
            .ThenBy(g => g.Key, StringComparer.Ordinal)
            .Select(g => $"{g.Count()}x {g.Key}")
            .ToList();
    }
}