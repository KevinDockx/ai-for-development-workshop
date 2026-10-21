using ClaimHub.Domain.Entities;
using ClaimHub.Domain.Enums;
using ClaimHub.Domain.Models;

namespace ClaimHub.Domain.Services;

public class ClaimTimelineBuilder
{
    public IReadOnlyList<ClaimTimelineEntry> Build(Claim claim)
    {
        ArgumentNullException.ThrowIfNull(claim);

        var entries = new List<ClaimTimelineEntry>
        {
            new(claim.ReportedAt, ClaimTimelineEventKind.Reported,
                claim.IsLateReported ? "Claim reported late" : "Claim reported",
                $"Date of loss {claim.DateOfLoss:yyyy-MM-dd}")
        };

        entries.AddRange(claim.StatusHistory.Select(h => new ClaimTimelineEntry(
            h.ChangedAt,
            ClaimTimelineEventKind.StatusChanged,
            $"{h.FromStatus} to {h.ToStatus}",
            string.IsNullOrWhiteSpace(h.Note) ? $"By {h.ChangedBy}" : $"By {h.ChangedBy}: {h.Note}")));

        entries.AddRange(claim.Documents.Select(d => new ClaimTimelineEntry(
            d.UploadedAt, ClaimTimelineEventKind.DocumentUploaded, $"{d.Type} uploaded", d.FileName)));

        entries.AddRange(claim.FraudFlags.Select(f => new ClaimTimelineEntry(
            f.RaisedAt, ClaimTimelineEventKind.FraudFlagRaised, $"Flag: {f.Type}", f.Details)));

        if (claim.EscalatedAt is { } escalatedAt)
        {
            entries.Add(new ClaimTimelineEntry(escalatedAt, ClaimTimelineEventKind.Escalated, "Escalated to a senior claims handler"));
        }

        if (claim.PaidAt is { } paidAt && claim.PaidAmount > 0m)
        {
            entries.Add(new ClaimTimelineEntry(paidAt, ClaimTimelineEventKind.PaidOut, "Payout completed", $"{claim.PaidAmount:0.00} EUR"));
        }

        return entries
            .OrderBy(e => e.At)
            .ThenBy(e => e.Kind)
            .ToList();
    }

    public TimeSpan? TimeToFirstAssessment(Claim claim) =>
        claim.FirstAssessmentAt is { } assessedAt ? assessedAt - claim.ReportedAt : null;

    public TimeSpan Age(Claim claim, DateTimeOffset now) =>
        (claim.IsFinal ? claim.UpdatedAt ?? now : now) - claim.ReportedAt;
}
