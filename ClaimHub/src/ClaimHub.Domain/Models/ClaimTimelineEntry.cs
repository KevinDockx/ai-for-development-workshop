using ClaimHub.Domain.Enums;

namespace ClaimHub.Domain.Models;

public sealed record ClaimTimelineEntry(DateTimeOffset At, ClaimTimelineEventKind Kind, string Title, string? Detail = null);
