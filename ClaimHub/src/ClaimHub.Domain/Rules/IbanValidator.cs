using System.Text.RegularExpressions;

namespace ClaimHub.Domain.Rules;

public static partial class IbanValidator
{
    [GeneratedRegex("^[A-Z]{2}[0-9]{2}[A-Z0-9]{11,30}$")]
    private static partial Regex IbanPattern();

    public static bool IsValid(string? iban) =>
        !string.IsNullOrWhiteSpace(iban) && IbanPattern().IsMatch(iban);

    public static bool IsBelgian(string? iban) =>
        IsValid(iban) && iban!.StartsWith("BE", StringComparison.Ordinal) && iban.Length == 16;

    public static string Mask(string iban) =>
        iban.Length <= 8 ? new string('*', iban.Length) : iban[..4] + new string('*', iban.Length - 8) + iban[^4..];
}
