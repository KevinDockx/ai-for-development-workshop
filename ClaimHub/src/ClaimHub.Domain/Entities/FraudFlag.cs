using ClaimHub.Domain.Enums;

namespace ClaimHub.Domain.Entities;

public class FraudFlag
{
    public int Id { get; set; }

    public int ClaimId { get; set; }

    public virtual Claim? Claim { get; set; }

    public FraudFlagType Type { get; set; }

    public string Details { get; set; } = string.Empty;

    public int Weight { get; set; }

    public DateTimeOffset RaisedAt { get; set; }

    public bool IsReviewed { get; set; }

    public string? ReviewedBy { get; set; }
}
