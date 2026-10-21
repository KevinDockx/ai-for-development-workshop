using ClaimHub.Domain.Enums;

namespace ClaimHub.Contracts.Policies;

public class InstallmentDto
{
    public int Number { get; set; }

    public DateOnly DueDate { get; set; }

    public decimal Amount { get; set; }
}

public class DiscountLineDto
{
    public string Name { get; set; } = string.Empty;

    public double Rate { get; set; }
}

public class RatingFactorDto
{
    public string Name { get; set; } = string.Empty;

    public string Band { get; set; } = string.Empty;

    public double Value { get; set; }
}

public class QuoteResponse
{
    public ProductType ProductType { get; set; }

    public decimal BasePremium { get; set; }

    public decimal RiskFactor { get; set; }

    public decimal DiscountPercent { get; set; }

    public decimal NetPremium { get; set; }

    public decimal TaxRate { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal AnnualPremium { get; set; }

    public PaymentFrequency PaymentFrequency { get; set; }

    public Dictionary<CoverageType, decimal> CoveragePremiums { get; set; } = new();

    public List<InstallmentDto> Installments { get; set; } = new();

    public List<DiscountLineDto> Discounts { get; set; } = new();

    public List<RatingFactorDto> RatingFactors { get; set; } = new();
}

public class CoverageDto
{
    public int Id { get; set; }

    public CoverageType Type { get; set; }

    public decimal Limit { get; set; }

    public decimal Deductible { get; set; }

    public decimal Premium { get; set; }

    public bool IsActive { get; set; }
}

public class EndorsementDto
{
    public int Id { get; set; }

    public EndorsementType Type { get; set; }

    public DateOnly EffectiveDate { get; set; }

    public string Description { get; set; } = string.Empty;

    public decimal OldAnnualPremium { get; set; }

    public decimal NewAnnualPremium { get; set; }

    public decimal PremiumDelta { get; set; }

    public string CreatedBy { get; set; } = string.Empty;
}

public class PolicyListItemDto
{
    public int Id { get; set; }

    public string PolicyNumber { get; set; } = string.Empty;

    public ProductType ProductType { get; set; }

    public PolicyStatus Status { get; set; }

    public int PolicyHolderId { get; set; }

    public string? PolicyHolderName { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public decimal AnnualPremium { get; set; }
}

public class PolicyDto : PolicyListItemDto
{
    public int? BrokerId { get; set; }

    public string? BrokerName { get; set; }

    public DateOnly? CancelledOn { get; set; }

    public string? CancellationReason { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal DiscountPercent { get; set; }

    public PaymentFrequency PaymentFrequency { get; set; }

    public string RiskSummary { get; set; } = string.Empty;

    public List<CoverageDto> Coverages { get; set; } = new();

    public List<EndorsementDto> Endorsements { get; set; } = new();
}

public class EndorsementResultDto
{
    public int PolicyId { get; set; }

    public int EndorsementId { get; set; }

    public DateOnly EffectiveDate { get; set; }

    public int RemainingDays { get; set; }

    public decimal OldAnnualPremium { get; set; }

    public decimal NewAnnualPremium { get; set; }

    public decimal AdditionalPremium { get; set; }

    public bool IsCredit { get; set; }
}

public class CancellationResultDto
{
    public int PolicyId { get; set; }

    public PolicyStatus Status { get; set; }

    public DateOnly EffectiveDate { get; set; }

    public int TermDays { get; set; }

    public int UnusedDays { get; set; }

    public decimal RefundAmount { get; set; }

    public string? Message { get; set; }

    public string? CreditNoteNumber { get; set; }
}
