using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace ClaimHub.Infrastructure.External;

public class FakePaymentGateway : IPaymentGateway
{
    private readonly PaymentGatewayOptions _options;
    private readonly ConcurrentDictionary<string, PaymentGatewayResult> _processed = new();

    public FakePaymentGateway(IOptions<PaymentGatewayOptions> options)
    {
        _options = options.Value;
    }

    public async Task<PaymentGatewayResult> SubmitAsync(PaymentGatewayRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!string.IsNullOrEmpty(request.IdempotencyKey) && _processed.TryGetValue(request.IdempotencyKey, out var previous))
        {
            return previous;
        }

        await Task.Delay(Random.Shared.Next(_options.MinLatencyMs, _options.MaxLatencyMs + 1), cancellationToken);

        var result = Evaluate(request);
        if (!string.IsNullOrEmpty(request.IdempotencyKey))
        {
            result = _processed.GetOrAdd(request.IdempotencyKey, result);
        }

        return result;
    }

    private PaymentGatewayResult Evaluate(PaymentGatewayRequest request)
    {
        var now = DateTimeOffset.UtcNow;

        if (request.Amount <= 0m)
        {
            return new PaymentGatewayResult(false, null, "Amount must be positive.", now);
        }

        if (request.Amount > _options.MaxAmount)
        {
            return new PaymentGatewayResult(false, null, "Amount exceeds the gateway limit.", now);
        }

        if (string.IsNullOrWhiteSpace(request.Iban))
        {
            return new PaymentGatewayResult(false, null, "Missing account number.", now);
        }

        return new PaymentGatewayResult(true, ReferenceFor(request), null, now);
    }

    private static string ReferenceFor(PaymentGatewayRequest request)
    {
        string source = $"{request.Direction}|{request.Reference}|{request.Amount:0.00}";
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(source));
        return "GW-" + Convert.ToHexString(hash, 0, 6);
    }
}
