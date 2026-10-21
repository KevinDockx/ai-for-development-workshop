using System.Text.RegularExpressions;

namespace ClaimHub.Domain.Rules;

public static partial class NationalNumberValidator
{
    [GeneratedRegex(@"^\d{2}\.(0[1-9]|1[0-2])\.(0[1-9]|[12]\d|3[01])-\d{3}\.\d{2}$")]
    private static partial Regex NationalNumberPattern();

    public static bool IsValidFormat(string? value) =>
        !string.IsNullOrWhiteSpace(value) && NationalNumberPattern().IsMatch(value);

    public static string Mask(string value) =>
        value.Length < 6 ? "***" : value[..3] + new string('*', value.Length - 6) + value[^3..];
}
