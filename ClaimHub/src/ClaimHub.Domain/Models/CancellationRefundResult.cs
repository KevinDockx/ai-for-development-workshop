namespace ClaimHub.Domain.Models;

public class CancellationRefundResult
{
    public decimal AnnualPremium { get; set; }

    public DateOnly EffectiveDate { get; set; }

    public int UnusedDays { get; set; }

    public int TermDays { get; set; }

    public decimal RefundAmount { get; set; }

    public string? Message { get; set; }

    public bool HasRefund => RefundAmount > 0m;
}
