namespace ClaimHub.Domain.Models;

public class ClaimEligibilityResult
{
    public bool IsEligible { get; set; }

    public bool IsLateReported { get; set; }

    public int DaysBetweenLossAndReport { get; set; }

    public IList<string> Reasons { get; set; } = new List<string>();

    public static ClaimEligibilityResult Rejected(string reason) =>
        new() { IsEligible = false, Reasons = new List<string> { reason } };
}
