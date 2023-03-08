using ClaimHub.Domain.Common;
using ClaimHub.Domain.Enums;

namespace ClaimHub.Domain.Entities;

public class Claim : EntityBase, IAuditable, ISoftDeletable
{
    public string ClaimNumber { get; set; } = string.Empty;

    public int PolicyId { get; set; }

    public virtual Policy? Policy { get; set; }

    public int? CoverageId { get; set; }

    public virtual Coverage? Coverage { get; set; }

    public ClaimStatus Status { get; set; } = ClaimStatus.Reported;

    public DateOnly DateOfLoss { get; set; }

    public DateTimeOffset ReportedAt { get; set; }

    public string Description { get; set; } = string.Empty;

    public bool IsLateReported { get; set; }

    public decimal? AssessedAmount { get; set; }

    public decimal? ApprovedAmount { get; set; }

    public decimal PaidAmount { get; set; }

    public DateTimeOffset? PaidAt { get; set; }

    public int? AssignedHandlerId { get; set; }

    public virtual ApplicationUser? AssignedHandler { get; set; }

    public DateTimeOffset? FirstAssessmentAt { get; set; }

    public DateTimeOffset? EscalatedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public DateTimeOffset? DeletedAt { get; set; }

    public string? DeletedBy { get; set; }

    public virtual ICollection<ClaimStatusHistory> StatusHistory { get; set; } = new List<ClaimStatusHistory>();

    public virtual ICollection<FraudFlag> FraudFlags { get; set; } = new List<FraudFlag>();

    public virtual ICollection<ClaimDocument> Documents { get; set; } = new List<ClaimDocument>();

    public bool IsFinal => Status is ClaimStatus.Closed or ClaimStatus.Withdrawn;

    public bool IsPaid => PaidAmount > 0m && Status is ClaimStatus.PaidOut or ClaimStatus.Closed;

    public void SoftDelete(string deletedBy, DateTimeOffset at)
    {
        IsDeleted = true;
        DeletedAt = at;
        DeletedBy = deletedBy;
    }

    public void RegisterAssessment(DateTimeOffset at)
    {
        FirstAssessmentAt ??= at;
        UpdatedAt = at;
    }
}
