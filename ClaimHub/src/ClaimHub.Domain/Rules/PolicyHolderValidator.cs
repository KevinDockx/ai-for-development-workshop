using System.Net.Mail;
using ClaimHub.Domain.Entities;
using ClaimHub.Domain.Models;

namespace ClaimHub.Domain.Rules;

public class PolicyHolderValidator
{
    public ValidationReport Validate(PolicyHolder holder, DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(holder);

        var report = new ValidationReport();
        report.AddIf(string.IsNullOrWhiteSpace(holder.FirstName), nameof(holder.FirstName), "The first name is required.");
        report.AddIf(string.IsNullOrWhiteSpace(holder.LastName), nameof(holder.LastName), "The last name is required.");
        report.AddIf(!IsValidEmail(holder.Email), nameof(holder.Email), "The e-mail address is not valid.");
        report.AddIf(!string.IsNullOrWhiteSpace(holder.Phone) && !IsValidPhone(holder.Phone), nameof(holder.Phone), "The phone number is not valid.");

        if (!NationalNumberValidator.IsValidFormat(holder.NationalNumber))
        {
            report.Add(nameof(holder.NationalNumber), "The national number must use the format YY.MM.DD-XXX.CC.");
        }
        else if (!MatchesBirthDate(holder.NationalNumber, holder.DateOfBirth))
        {
            report.Add(nameof(holder.NationalNumber), "The national number does not match the date of birth.");
        }

        report.AddIf(!IbanValidator.IsValid(holder.Iban), nameof(holder.Iban), "The IBAN is not valid.");
        report.AddIf(string.IsNullOrWhiteSpace(holder.Street), nameof(holder.Street), "The street is required.");
        report.AddIf(!IsValidPostalCode(holder.PostalCode), nameof(holder.PostalCode), "A Belgian postal code has four digits.");
        report.AddIf(string.IsNullOrWhiteSpace(holder.City), nameof(holder.City), "The city is required.");
        report.AddIf(holder.CustomerSince > today, nameof(holder.CustomerSince), "The customer-since date cannot lie in the future.");
        report.AddIf(holder.CreditLimit < 0m, nameof(holder.CreditLimit), "The credit limit cannot be negative.");
        return report;
    }

    public static bool IsValidEmail(string? email) =>
        !string.IsNullOrWhiteSpace(email)
        && MailAddress.TryCreate(email, out var address)
        && string.Equals(address.Address, email.Trim(), StringComparison.OrdinalIgnoreCase)
        && address.Host.Contains('.');

    public static bool IsValidPostalCode(string? postalCode) =>
        postalCode is { Length: 4 } && postalCode.All(char.IsAsciiDigit) && postalCode[0] != '0';

    public static bool IsValidPhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return false;
        }

        int digits = phone.Count(char.IsAsciiDigit);
        return digits is >= 8 and <= 15 && phone.All(c => char.IsAsciiDigit(c) || c is '+' or ' ' or '/' or '.' or '-');
    }

    // The century is not part of the national number, so only the two-digit year, month and day are compared.
    public static bool MatchesBirthDate(string nationalNumber, DateOnly dateOfBirth)
    {
        if (!NationalNumberValidator.IsValidFormat(nationalNumber))
        {
            return false;
        }

        int year = int.Parse(nationalNumber.AsSpan(0, 2));
        int month = int.Parse(nationalNumber.AsSpan(3, 2));
        int day = int.Parse(nationalNumber.AsSpan(6, 2));
        return dateOfBirth.Year % 100 == year && dateOfBirth.Month == month && dateOfBirth.Day == day;
    }
}
