using ClaimHub.Domain.Enums;

namespace ClaimHub.Infrastructure.External;

public record PaymentGatewayRequest(
    PaymentDirection Direction,
    decimal Amount,
    string Reference,
    string Iban,
    string IdempotencyKey = null);

public record PaymentGatewayResult(
    bool Succeeded,
    string GatewayReference,
    string FailureReason,
    DateTimeOffset ProcessedAt);

public interface IPaymentGateway
{
    /// <summary>Collects (incoming) or pays out (outgoing) money. Repeating an idempotency key returns the first result.</summary>
    Task<PaymentGatewayResult> SubmitAsync(PaymentGatewayRequest request, CancellationToken cancellationToken = default);
}
