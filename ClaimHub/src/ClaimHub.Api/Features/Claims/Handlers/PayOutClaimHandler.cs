using ClaimHub.Contracts.Claims;
using ClaimHub.Domain.Common;
using ClaimHub.Domain.Entities;
using ClaimHub.Domain.Enums;
using ClaimHub.Domain.Rules;
using ClaimHub.Infrastructure.Data;
using ClaimHub.Infrastructure.External;
using ClaimHub.Infrastructure.Mediator;

namespace ClaimHub.Api.Features.Claims.Handlers;

public class PayOutClaimHandler : IRequestHandler<PayOutClaimCommand, Result<PayoutDto>>
{
    private readonly ClaimHubDbContext _db;
    private readonly IPaymentGateway _gateway;
    private readonly ClaimStateMachine _stateMachine;
    private readonly ClaimDetailBuilder _details;

    public PayOutClaimHandler(
        ClaimHubDbContext db,
        IPaymentGateway gateway,
        ClaimStateMachine stateMachine,
        ClaimDetailBuilder details)
    {
        _db = db;
        _gateway = gateway;
        _stateMachine = stateMachine;
        _details = details;
    }

    public async Task<Result<PayoutDto>> HandleAsync(PayOutClaimCommand request, CancellationToken cancellationToken)
    {
        var claim = await _details.LoadAsync(request.ClaimId, cancellationToken);
        if (claim == null)
        {
            return Result.Failure<PayoutDto>($"Claim {request.ClaimId} was not found.", "claim.not_found");
        }

        var validation = _stateMachine.Validate(claim, ClaimStatus.PaidOut);
        if (!validation.IsAllowed)
        {
            return Result.Failure<PayoutDto>(validation.Reason ?? "Transition not allowed.", "claim.invalid_transition");
        }

        decimal amount = claim.ApprovedAmount ?? 0m;
        if (amount <= 0m)
        {
            return Result.Failure<PayoutDto>("The claim has no approved amount to pay out.", "claim.nothing_to_pay");
        }

        var iban = await FindIbanAsync(claim, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var payment = Payment.PayoutFor(claim, amount, now);
        var gatewayResult = Submit(claim, amount, iban);

        if (!gatewayResult.Succeeded)
        {
            payment.Fail(gatewayResult.FailureReason ?? "Payout was declined.");
            _db.Payments.Add(payment);
            await _db.SaveChangesAsync(cancellationToken);
            return Result.Failure<PayoutDto>(payment.FailureReason!, "claim.payout_failed");
        }

        payment.Complete(gatewayResult.GatewayReference, gatewayResult.ProcessedAt);
        _db.Payments.Add(payment);

        claim.PaidAmount += amount;
        claim.PaidAt = gatewayResult.ProcessedAt;
        _stateMachine.Apply(claim, ClaimStatus.PaidOut, request.Actor.Email, now, "Paid out");
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success(new PayoutDto(claim.Id, claim.ClaimNumber, amount, payment.GatewayReference, claim.Status));
    }

    private async Task<string> FindIbanAsync(Claim claim, CancellationToken cancellationToken)
    {
        var holder = await _db.PolicyHolders.FindAsync(new object[] { claim.Policy!.PolicyHolderId }, cancellationToken);
        return holder?.Iban ?? string.Empty;
    }

    private PaymentGatewayResult Submit(Claim claim, decimal amount, string iban)
    {
        var request = new PaymentGatewayRequest(PaymentDirection.Outgoing, amount, claim.ClaimNumber, iban);
        return _gateway.SubmitAsync(request).Result;
    }
}
