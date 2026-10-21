using ClaimHub.Domain.Entities;
using ClaimHub.Domain.Enums;
using ClaimHub.Domain.Models;

namespace ClaimHub.Domain.Rules;

public class RiskValidator
{
    public ValidationReport ValidateFor(Policy policy, DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(policy);

        var report = new ValidationReport();
        int attached = new object?[] { policy.HomeRisk, policy.MotorRisk, policy.TravelRisk }.Count(r => r is not null);
        report.AddIf(attached > 1, "Risk", "A policy can only carry the risk of its own product.");

        switch (policy.ProductType)
        {
            case ProductType.Home:
                return policy.HomeRisk is null ? report.Add("Risk", "A home policy needs a home risk.") : report.Merge(Validate(policy.HomeRisk, today));
            case ProductType.Motor:
                return policy.MotorRisk is null ? report.Add("Risk", "A motor policy needs a motor risk.") : report.Merge(Validate(policy.MotorRisk, today));
            case ProductType.Travel:
                return policy.TravelRisk is null ? report.Add("Risk", "A travel policy needs a travel risk.") : report.Merge(Validate(policy.TravelRisk));
            default:
                return report;
        }
    }

    public ValidationReport Validate(HomeRisk risk, DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(risk);

        var report = new ValidationReport();
        report.AddIf(string.IsNullOrWhiteSpace(risk.Street), nameof(risk.Street), "The street is required.");
        report.AddIf(!PolicyHolderValidator.IsValidPostalCode(risk.PostalCode), nameof(risk.PostalCode), "A Belgian postal code has four digits.");
        report.AddIf(string.IsNullOrWhiteSpace(risk.City), nameof(risk.City), "The city is required.");
        report.AddIf(!risk.HasValidZone(), nameof(risk.PostcodeZone), "The postcode zone must be between 1 and 4.");
        report.AddIf(risk.BuildingValue < 0m, nameof(risk.BuildingValue), "The building value cannot be negative.");
        report.AddIf(risk.ContentsValue < 0m, nameof(risk.ContentsValue), "The contents value cannot be negative.");
        report.AddIf(risk.BuildingValue == 0m && risk.ContentsValue == 0m, nameof(risk.BuildingValue), "Either the building or the contents needs a value.");
        return report;
    }

    public ValidationReport Validate(MotorRisk risk, DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(risk);

        var report = new ValidationReport();
        report.AddIf(string.IsNullOrWhiteSpace(risk.LicensePlate), nameof(risk.LicensePlate), "The license plate is required.");
        report.AddIf(string.IsNullOrWhiteSpace(risk.Make), nameof(risk.Make), "The make is required.");
        report.AddIf(string.IsNullOrWhiteSpace(risk.Model), nameof(risk.Model), "The model is required.");
        report.AddIf(risk.VehicleValue <= 0m, nameof(risk.VehicleValue), "The vehicle value must be positive.");
        report.AddIf(!risk.HasValidPower(), nameof(risk.PowerKw), "The engine power must be positive.");
        report.AddIf(risk.ClaimFreeYears < 0, nameof(risk.ClaimFreeYears), "The number of claim-free years cannot be negative.");
        return report;
    }

    public ValidationReport Validate(TravelRisk risk)
    {
        ArgumentNullException.ThrowIfNull(risk);

        var report = new ValidationReport();
        report.AddIf(!Enum.IsDefined(risk.Scope), nameof(risk.Scope), "The travel scope is unknown.");
        return report;
    }
}
