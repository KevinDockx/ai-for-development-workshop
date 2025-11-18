namespace ClaimHub.Infrastructure.External;

public record IbanCheckResult(bool IsValid, string BankName, string Reason);

public interface IIbanCheckService
{
    Task<IbanCheckResult> CheckAsync(string iban, CancellationToken cancellationToken = default);
}
