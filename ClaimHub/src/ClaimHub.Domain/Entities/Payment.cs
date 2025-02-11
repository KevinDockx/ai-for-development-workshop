using ClaimHub.Domain.Common;
using ClaimHub.Domain.Enums;

namespace ClaimHub.Domain.Entities;

public class Payment : EntityBase
{
    public PaymentDirection Direction { get; set; } = PaymentDirection.Incoming;

    public int? InvoiceId { get; set; }

    public virtual Invoice? Invoice { get; set; }

    public int? ClaimId { get; set; }

    public virtual Claim? Claim { get; set; }

    public decimal Amount { get; set; }

    public PaymentMethod Method { get; set; } = PaymentMethod.DirectDebit;

    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    public string? GatewayReference { get; set; }

    public string? IdempotencyKey { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }

    public string? FailureReason { get; set; }

    public static Payment ForInvoice(Invoice invoice, decimal amount, PaymentMethod method, DateTimeOffset at)
    {
        if (amount <= 0m)
        {
            throw new DomainRuleException("A payment amount must be positive.");
        }

        return new Payment
        {
            Direction = PaymentDirection.Incoming,
            InvoiceId = invoice.Id,
            Amount = MoneyMath.ToCents(amount),
            Method = method,
            CreatedAt = at
        };
    }

    public static Payment PayoutFor(Claim claim, decimal amount, DateTimeOffset at)
    {
        if (amount <= 0m)
        {
            throw new DomainRuleException("A payout amount must be positive.");
        }

        return new Payment
        {
            Direction = PaymentDirection.Outgoing,
            ClaimId = claim.Id,
            Amount = MoneyMath.ToCents(amount),
            Method = PaymentMethod.BankTransfer,
            CreatedAt = at
        };
    }

    public void Complete(string gatewayReference, DateTimeOffset at)
    {
        if (Status != PaymentStatus.Pending)
        {
            throw new DomainRuleException($"A payment in status {Status} cannot be completed.");
        }

        Status = PaymentStatus.Completed;
        GatewayReference = gatewayReference;
        CompletedAt = at;
    }

    public void Fail(string reason)
    {
        if (Status != PaymentStatus.Pending)
        {
            throw new DomainRuleException($"A payment in status {Status} cannot fail.");
        }

        Status = PaymentStatus.Failed;
        FailureReason = reason;
    }
}
