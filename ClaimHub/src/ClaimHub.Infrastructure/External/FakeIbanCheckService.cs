using ClaimHub.Domain.Rules;

namespace ClaimHub.Infrastructure.External;

public class FakeIbanCheckService : IIbanCheckService
{
    private static readonly Dictionary<string, string> Banks = new()
    {
        ["999"] = "Demo Bank Scheldt",
        ["001"] = "Voorbeeld Spaarbank",
        ["733"] = "Proef Handelsbank"
    };

    public Task<IbanCheckResult> CheckAsync(string iban, CancellationToken cancellationToken = default)
    {
        if (!IbanValidator.IsValid(iban))
        {
            return Task.FromResult(new IbanCheckResult(false, null, "The IBAN format is not valid."));
        }

        string bank = null;
        if (IbanValidator.IsBelgian(iban))
        {
            Banks.TryGetValue(iban.Substring(4, 3), out bank);
        }

        return Task.FromResult(new IbanCheckResult(true, bank ?? "Unknown bank", null));
    }
}
