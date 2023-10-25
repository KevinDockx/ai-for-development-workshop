using ClaimHub.Domain.Common;
using ClaimHub.Domain.Enums;

namespace ClaimHub.Domain.Entities;

public class Policy : EntityBase, IAuditable, ISoftDeletable
{
    public string PolicyNumber { get; set; } = string.Empty;

    public ProductType ProductType { get; set; }

    public PolicyStatus Status { get; set; } = PolicyStatus.Quoted;

    public int PolicyHolderId { get; set; }

    public virtual PolicyHolder? PolicyHolder { get; set; }

    public int? BrokerId { get; set; }

    public virtual Broker? Broker { get; set; }

    public int? RenewedFromPolicyId { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public DateOnly? CancelledOn { get; set; }

    public string? CancellationReason { get; set; }

    public decimal AnnualPremium { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal DiscountPercent { get; set; }

    public PaymentFrequency PaymentFrequency { get; set; } = PaymentFrequency.Annual;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public DateTimeOffset? DeletedAt { get; set; }

    public string? DeletedBy { get; set; }

    public virtual ICollection<Coverage> Coverages { get; set; } = new List<Coverage>();

    public virtual ICollection<Claim> Claims { get; set; } = new List<Claim>();

    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    public virtual ICollection<PolicyEndorsement> Endorsements { get; set; } = new List<PolicyEndorsement>();

    public virtual ICollection<InsuredPerson> InsuredPersons { get; set; } = new List<InsuredPerson>();

    public virtual HomeRisk? HomeRisk { get; set; }

    public virtual MotorRisk? MotorRisk { get; set; }

    public virtual TravelRisk? TravelRisk { get; set; }

    public bool IsInForceOn(DateOnly date) =>
        Status == PolicyStatus.Active && date >= StartDate && date < EndDate;

    public int TermLengthInDays => EndDate.DayNumber - StartDate.DayNumber;

    public Coverage? FindCoverage(CoverageType type) =>
        Coverages.FirstOrDefault(c => c.Type == type && c.IsActive);

    public void Activate()
    {
        if (Status != PolicyStatus.Quoted && Status != PolicyStatus.Suspended)
        {
            throw new DomainRuleException($"A policy in status {Status} cannot be activated.");
        }

        Status = PolicyStatus.Active;
    }

    public void Suspend()
    {
        if (Status != PolicyStatus.Active)
        {
            throw new DomainRuleException("Only an active policy can be suspended.");
        }

        Status = PolicyStatus.Suspended;
    }

    public void Cancel(DateOnly effectiveOn, string reason)
    {
        if (Status != PolicyStatus.Active && Status != PolicyStatus.Suspended)
        {
            throw new DomainRuleException($"A policy in status {Status} cannot be cancelled.");
        }

        if (effectiveOn < StartDate)
        {
            throw new DomainRuleException("Cancellation cannot be effective before the start date.");
        }

        Status = PolicyStatus.Cancelled;
        CancelledOn = effectiveOn;
        CancellationReason = reason;
    }

    public void MarkRenewed()
    {
        if (Status != PolicyStatus.Active)
        {
            throw new DomainRuleException("Only an active policy can be renewed.");
        }

        Status = PolicyStatus.Renewed;
    }

    public void MarkExpired()
    {
        if (Status != PolicyStatus.Active)
        {
            throw new DomainRuleException("Only an active policy can expire.");
        }

        Status = PolicyStatus.Expired;
    }
}
