using ClaimHub.Domain.Enums;

namespace ClaimHub.Domain.Models;

public enum PremiumStepKind
{
    Base = 1,
    RiskAdjustment = 2,
    Discount = 3,
    Tax = 4,
    Total = 5,
    InstallmentSurcharge = 6
}

public sealed record PremiumExplanationLine(PremiumStepKind Kind, string Label, decimal Amount, string? Note = null);

public sealed record CoverageShare(CoverageType Type, decimal Premium, decimal SharePercentage);

public sealed class InstallmentAudit
{
    public int Count { get; init; }

    public decimal Total { get; init; }

    public decimal DifferenceToAnnual { get; init; }

    public decimal FirstAmount { get; init; }

    public decimal LastAmount { get; init; }

    public bool AllEqual => FirstAmount == LastAmount;

    public bool SumsToAnnual => DifferenceToAnnual == 0m;
}

public sealed class PremiumExplanation
{
    public ProductType ProductType { get; init; }

    public PaymentFrequency PaymentFrequency { get; init; }

    public IReadOnlyList<PremiumExplanationLine> Lines { get; init; } = Array.Empty<PremiumExplanationLine>();

    public IReadOnlyList<CoverageShare> CoverageShares { get; init; } = Array.Empty<CoverageShare>();

    public InstallmentAudit Installments { get; init; } = new();

    public decimal DiscountPercent { get; init; }

    public decimal RiskFactor { get; init; }

    public decimal AnnualPremium { get; init; }

    // What the displayed steps add up to minus the annual premium, caused by rounding each step to cents.
    public decimal RoundingResidue { get; init; }

    public decimal TotalDiscountAmount => Lines
        .Where(l => l.Kind == PremiumStepKind.Discount)
        .Sum(l => l.Amount);

    public IEnumerable<PremiumExplanationLine> LinesOfKind(PremiumStepKind kind) =>
        Lines.Where(l => l.Kind == kind);
}

public sealed record PremiumComparison(
    decimal PreviousAnnualPremium,
    decimal CurrentAnnualPremium,
    decimal AnnualDelta,
    decimal NetDelta,
    decimal TaxDelta,
    decimal RiskFactorDelta)
{
    public bool IsIncrease => AnnualDelta > 0m;
}