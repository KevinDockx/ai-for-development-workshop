using ClaimHub.Domain.Entities;
using ClaimHub.Domain.Enums;
using ClaimHub.Domain.Models;

namespace ClaimHub.Domain.Rules;

public class InsuredPersonValidator
{
    public ValidationReport Validate(InsuredPerson person, Policy policy, DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(person);
        ArgumentNullException.ThrowIfNull(policy);

        var report = new ValidationReport();
        report.AddIf(string.IsNullOrWhiteSpace(person.FirstName), nameof(person.FirstName), "The first name is required.");
        report.AddIf(string.IsNullOrWhiteSpace(person.LastName), nameof(person.LastName), "The last name is required.");
        report.AddIf(person.DateOfBirth > today, nameof(person.DateOfBirth), "The date of birth cannot lie in the future.");

        if (!string.IsNullOrWhiteSpace(person.NationalNumber))
        {
            if (!NationalNumberValidator.IsValidFormat(person.NationalNumber))
            {
                report.Add(nameof(person.NationalNumber), "The national number must use the format YY.MM.DD-XXX.CC.");
            }
            else if (!PolicyHolderValidator.MatchesBirthDate(person.NationalNumber, person.DateOfBirth))
            {
                report.Add(nameof(person.NationalNumber), "The national number does not match the date of birth.");
            }
        }

        return report;
    }
}
