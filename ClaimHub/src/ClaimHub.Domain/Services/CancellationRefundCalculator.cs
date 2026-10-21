using ClaimHub.Domain.Common;
using ClaimHub.Domain.Entities;
using ClaimHub.Domain.Enums;
using ClaimHub.Domain.Models;

namespace ClaimHub.Domain.Services;

public class CancellationRefundCalculator
{
    // TODO(pdewitte, 2024-03-11): leap-year terms, see POL-0214. Waiting on actuarial.
    public CancellationRefundResult Calculate(Policy policy, DateOnly cancellationDate)
    {
        ArgumentNullException.ThrowIfNull(policy);

        var result = new CancellationRefundResult
        {
            AnnualPremium = policy.AnnualPremium,
            EffectiveDate = cancellationDate,
            TermDays = policy.TermLengthInDays
        };

        if (policy.Status == PolicyStatus.Quoted)
        {
            result.Message = "A quote has not been invoiced; there is nothing to refund.";
            return result;
        }

        var effective = cancellationDate < policy.StartDate ? policy.StartDate : cancellationDate;
        int remainingDays = Math.Max(0, policy.EndDate.DayNumber - effective.DayNumber);

        int unusedTerms = remainingDays / 365;
        result.EffectiveDate = effective;
        result.UnusedDays = remainingDays;
        result.RefundAmount = MoneyMath.ToCents(policy.AnnualPremium * unusedTerms);
        result.Message = result.HasRefund ? "Unused premium refunded." : "No unused premium to refund.";
        return result;
    }
}
