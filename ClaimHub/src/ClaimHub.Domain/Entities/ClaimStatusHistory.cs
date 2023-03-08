using ClaimHub.Domain.Enums;

namespace ClaimHub.Domain.Entities;

public class ClaimStatusHistory
{
    public int Id { get; set; }

    public int ClaimId { get; set; }

    public virtual Claim? Claim { get; set; }

    public ClaimStatus FromStatus { get; set; }

    public ClaimStatus ToStatus { get; set; }

    public DateTimeOffset ChangedAt { get; set; }

    public string ChangedBy { get; set; } = string.Empty;

    public string? Note { get; set; }
}
