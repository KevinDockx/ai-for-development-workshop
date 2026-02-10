using ClaimHub.Contracts.Policies;
using ClaimHub.Domain.Common;
using ClaimHub.Domain.Entities;
using ClaimHub.Domain.Enums;
using ClaimHub.Domain.Models;
using ClaimHub.Domain.Rules;

namespace ClaimHub.Api.Policies;

public partial class PolicyService
{
    public async Task<QuoteResponse> QuoteAsync(QuotePolicyRequest request, CancellationToken cancellationToken)
    {
        var holder = await RequireHolderAsync(request.PolicyHolderId, cancellationToken);
        var draft = await BuildDraftAsync(request, holder, cancellationToken);
        var quote = await CalculateAsync(draft, holder, cancellationToken);

        return _mapper.Map<QuoteResponse>(quote);
    }

    private async Task<PremiumQuote> CalculateAsync(Policy policy, PolicyHolder holder, CancellationToken cancellationToken)
    {
        await EnsureRatingTableAsync(cancellationToken);
        var calculation = await ToCalculationRequestAsync(policy, holder, cancellationToken);
        return _premiums.Calculate(calculation);
    }

    private async Task<PremiumCalculationRequest> ToCalculationRequestAsync(
        Policy policy,
        PolicyHolder holder,
        CancellationToken cancellationToken)
    {
        int activePolicies = await _uow.Policies.CountAsync(
            p => p.PolicyHolderId == holder.Id && p.Status == PolicyStatus.Active && !p.IsDeleted,
            cancellationToken);

        var request = new PremiumCalculationRequest
        {
            ProductType = policy.ProductType,
            PaymentFrequency = policy.PaymentFrequency,
            StartDate = policy.StartDate,
            Coverages = policy.Coverages.Where(c => c.IsActive).Select(c => c.Type).ToList(),
            ActivePolicyCount = activePolicies,
            FullYearsAsCustomer = holder.FullYearsAsCustomer(Today)
        };

        switch (policy.ProductType)
        {
            case ProductType.Home when policy.HomeRisk is { } home:
                request.PostcodeZone = home.PostcodeZone;
                request.BuildingValue = home.BuildingValue;
                request.ContentsValue = home.ContentsValue;
                request.ConstructionYear = home.ConstructionYear;
                break;

            case ProductType.Motor when policy.MotorRisk is { } motor:
                request.DriverAge = _driverAges.Calculate(motor, policy.StartDate);
                request.VehicleValue = motor.VehicleValue;
                request.VehiclePowerKw = motor.PowerKw;
                request.ClaimFreeYears = motor.ClaimFreeYears;
                break;

            case ProductType.Travel when policy.TravelRisk is { } travel:
                request.TravelScope = travel.Scope;
                request.PartyType = travel.PartyType;
                break;
        }

        return request;
    }

    private async Task<Policy> BuildDraftAsync(QuotePolicyRequest request, PolicyHolder holder, CancellationToken cancellationToken)
    {
        var term = _terms.Calculate(request.StartDate);
        var policy = new Policy
        {
            ProductType = request.ProductType,
            PolicyHolderId = holder.Id,
            BrokerId = holder.BrokerId,
            StartDate = term.Start,
            EndDate = term.End,
            PaymentFrequency = request.PaymentFrequency,
            CreatedAt = Now
        };

        switch (request.ProductType)
        {
            case ProductType.Home:
                policy.HomeRisk = await BuildHomeRiskAsync(request.Home, cancellationToken);
                break;
            case ProductType.Motor:
                policy.MotorRisk = BuildMotorRisk(request.Motor);
                break;
            case ProductType.Travel:
                policy.TravelRisk = new TravelRisk
                {
                    Scope = request.Travel?.Scope ?? TravelScope.Europe,
                    PartyType = request.Travel?.PartyType ?? TravelPartyType.Single
                };
                break;
        }

        var selected = request.Coverages.Count > 0
            ? request.Coverages.Distinct().ToList()
            : DefaultCoverages(request.ProductType);

        ProductCoverageRules.Validate(request.ProductType, selected).ThrowIfInvalid();
        AddCoverages(policy, selected);
        return policy;
    }

    private async Task<HomeRisk> BuildHomeRiskAsync(HomeRiskInput input, CancellationToken cancellationToken)
    {
        if (input == null)
        {
            throw new DomainRuleException("Home details are required for a Home policy.");
        }

        // The address lookup knows the official risk zone of a municipality; fall back to the submitted zone.
        var suggestions = await _addresses.LookupByPostalCodeAsync(input.PostalCode, cancellationToken);
        int zone = suggestions.Count > 0 ? suggestions[0].RiskZone : input.PostcodeZone;

        return new HomeRisk
        {
            Street = input.Street,
            PostalCode = input.PostalCode,
            City = input.City,
            PostcodeZone = zone,
            BuildingValue = input.BuildingValue,
            ContentsValue = input.ContentsValue,
            ConstructionYear = input.ConstructionYear,
            IsOwnerOccupied = input.IsOwnerOccupied
        };
    }

    private static MotorRisk BuildMotorRisk(MotorRiskInput input)
    {
        if (input == null)
        {
            throw new DomainRuleException("Vehicle details are required for a Motor policy.");
        }

        return new MotorRisk
        {
            LicensePlate = input.LicensePlate.ToUpperInvariant(),
            Make = input.Make,
            Model = input.Model,
            FirstRegistrationYear = input.FirstRegistrationYear,
            VehicleValue = input.VehicleValue,
            PowerKw = input.PowerKw,
            DriverDateOfBirth = input.DriverDateOfBirth,
            ClaimFreeYears = input.ClaimFreeYears
        };
    }

    private static List<CoverageType> DefaultCoverages(ProductType product) => product switch
    {
        ProductType.Home => new List<CoverageType> { CoverageType.Building, CoverageType.Contents },
        ProductType.Motor => new List<CoverageType> { CoverageType.ThirdPartyLiability },
        _ => new List<CoverageType> { CoverageType.Travel }
    };

    private static void AddCoverages(Policy policy, IEnumerable<CoverageType> types)
    {
        foreach (var type in types)
        {
            var (limit, deductible) = LimitsFor(policy, type);
            policy.Coverages.Add(new Coverage
            {
                Type = type,
                Limit = limit,
                Deductible = deductible,
                IsActive = true
            });
        }
    }

    private static (decimal Limit, decimal Deductible) LimitsFor(Policy policy, CoverageType type) => type switch
    {
        CoverageType.Building => (policy.HomeRisk?.BuildingValue ?? 0m, 250m),
        CoverageType.Contents => (policy.HomeRisk?.ContentsValue ?? 0m, 150m),
        CoverageType.ThirdPartyLiability => (10_000_000m, 0m),
        CoverageType.Comprehensive => (policy.MotorRisk?.VehicleValue ?? 0m, 300m),
        CoverageType.Glass => (1_500m, 50m),
        CoverageType.Travel => (policy.TravelRisk?.Scope == TravelScope.Worldwide ? 100_000m : 50_000m, 100m),
        _ => (0m, 0m)
    };

    private static void ApplyQuote(Policy policy, PremiumQuote quote)
    {
        policy.AnnualPremium = quote.AnnualPremium;
        policy.TaxAmount = quote.TaxAmount;
        policy.DiscountPercent = quote.DiscountPercent;

        foreach (var coverage in policy.Coverages)
        {
            coverage.Premium = quote.CoveragePremiums.TryGetValue(coverage.Type, out decimal premium) ? premium : 0m;
        }
    }
}
