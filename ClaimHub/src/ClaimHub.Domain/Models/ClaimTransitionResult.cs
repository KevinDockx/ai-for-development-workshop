using ClaimHub.Domain.Enums;

namespace ClaimHub.Domain.Models;

public class ClaimTransitionResult
{
    public bool IsAllowed { get; set; }

    public ClaimStatus From { get; set; }

    public ClaimStatus To { get; set; }

    public string? Reason { get; set; }

    public static ClaimTransitionResult Allowed(ClaimStatus from, ClaimStatus to) =>
        new() { IsAllowed = true, From = from, To = to };

    public static ClaimTransitionResult Denied(ClaimStatus from, ClaimStatus to, string reason) =>
        new() { IsAllowed = false, From = from, To = to, Reason = reason };
}
