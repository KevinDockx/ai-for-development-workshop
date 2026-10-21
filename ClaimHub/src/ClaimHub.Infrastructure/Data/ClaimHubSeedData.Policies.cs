using ClaimHub.Domain.Entities;
using ClaimHub.Domain.Enums;
using ClaimHub.Domain.Services;
using ClaimHub.Infrastructure.External;

namespace ClaimHub.Infrastructure.Data;

public partial class ClaimHubSeedData
{
    private const decimal InsuranceTaxRate = 0.0925m;

    private static readonly (string Make, string Model)[] Vehicles =
    {
        ("Demo Motors", "Stad 1.0"), ("Proef Auto", "Familie 1.6"), ("Fictie Cars", "Sport 2.0"),
        ("Modelwagen", "Kombi 1.5"), ("Testmobiel", "Elektro 150")
    };

    private static readonly string[] CancellationReasons =
    {
        "Policyholder switched to another insurer", "Policyholder moved abroad",
        "Vehicle sold", "Property sold", "Cancelled after non-payment"
    };

    private static readonly decimal[] DiscountSteps = { 0m, 0m, 5m, 10m, 10m, 15m, 20m, 30m };

    private void CreatePolicies()
    {
        int regular = PolicyCount - QuotedPolicyCount;

        for (int i = 0; i < regular; i++)
        {
            // Every holder gets one policy. The next four go to the two login holders so each of them
            // ends up with a Home, Motor and Travel policy; the rest are spread at random.
            bool loginPolicy = i < 2 || (i >= PolicyHolders.Count && i < PolicyHolders.Count + 4);
            var holder = i < PolicyHolders.Count ? PolicyHolders[i]
                : loginPolicy ? PolicyHolders[i % 2]
                : Pick(PolicyHolders);

            var product = i < 2 ? ProductType.Home
                : i < PolicyHolders.Count ? RandomProduct()
                : loginPolicy ? (i < PolicyHolders.Count + 2 ? ProductType.Motor : ProductType.Travel)
                : RandomProduct();

            Policies.Add(CreatePolicy(holder, product, loginPolicy, quoted: false));
        }

        for (int i = 0; i < QuotedPolicyCount; i++)
        {
            Policies.Add(CreatePolicy(Pick(PolicyHolders), RandomProduct(), forceActive: false, quoted: true));
        }

        NumberPolicies();
    }

    private ProductType RandomProduct()
    {
        double roll = _random.NextDouble();
        return roll < 0.35 ? ProductType.Home : roll < 0.80 ? ProductType.Motor : ProductType.Travel;
    }

    private Policy CreatePolicy(PolicyHolder holder, ProductType product, bool forceActive, bool quoted)
    {
        var start = quoted ? Today.AddDays(Between(5, 45))
            : forceActive ? Today.AddDays(-Between(30, 300))
            : Today.AddDays(-Between(1, 729));

        var policy = new Policy
        {
            ProductType = product,
            PolicyHolder = holder,
            Broker = holder.Broker,
            StartDate = start,
            EndDate = start.AddYears(1),
            DiscountPercent = Pick(DiscountSteps),
            PaymentFrequency = Pick(new[] { PaymentFrequency.Annual, PaymentFrequency.Annual, PaymentFrequency.Quarterly, PaymentFrequency.Monthly }),
            CreatedAt = At(quoted ? Today.AddDays(-Between(1, 10)) : start.AddDays(-Between(1, 10)), 10)
        };

        if (quoted)
        {
            policy.Status = PolicyStatus.Quoted;
        }
        else
        {
            ApplyStatus(policy, forceActive);
        }

        switch (product)
        {
            case ProductType.Home:
                AddHome(policy, holder);
                break;
            case ProductType.Motor:
                AddMotor(policy, holder);
                break;
            default:
                AddTravel(policy, holder);
                break;
        }

        ApplyPremium(policy);
        return policy;
    }

    private void ApplyStatus(Policy policy, bool forceActive)
    {
        bool ended = policy.EndDate <= Today;
        double roll = _random.NextDouble();

        if (forceActive)
        {
            policy.Status = PolicyStatus.Active;
            return;
        }

        policy.Status = ended
            ? roll < 0.65 ? PolicyStatus.Expired : roll < 0.90 ? PolicyStatus.Renewed : PolicyStatus.Cancelled
            : roll < 0.90 ? PolicyStatus.Active : roll < 0.96 ? PolicyStatus.Cancelled : PolicyStatus.Suspended;

        if (policy.Status == PolicyStatus.Suspended && policy.StartDate > Today.AddDays(-120))
        {
            policy.Status = PolicyStatus.Active;
        }

        if (policy.Status != PolicyStatus.Cancelled)
        {
            return;
        }

        var earliest = policy.StartDate.AddDays(30);
        var latest = ended ? policy.EndDate.AddDays(-1) : Today.AddDays(-3);
        if (earliest > latest)
        {
            policy.Status = ended ? PolicyStatus.Expired : PolicyStatus.Active;
            return;
        }

        policy.CancelledOn = earliest.AddDays(_random.Next(0, latest.DayNumber - earliest.DayNumber + 1));
        policy.CancellationReason = Pick(CancellationReasons);
    }

    private void AddHome(Policy policy, PolicyHolder holder)
    {
        var town = FakeAddressLookup.Municipalities.First(m => m.PostalCode == holder.PostalCode);
        decimal buildingValue = Between(30, 130) * 5000m;
        decimal contentsValue = Between(10, 80) * 1000m;
        int constructionYear = Between(1920, 2022);

        policy.HomeRisk = new HomeRisk
        {
            Street = holder.Street,
            PostalCode = holder.PostalCode,
            City = holder.City,
            PostcodeZone = town.RiskZone,
            BuildingValue = buildingValue,
            ContentsValue = contentsValue,
            ConstructionYear = constructionYear,
            IsOwnerOccupied = Chance(0.85)
        };

        double zoneFactor = town.RiskZone switch { <= 1 => 0.9, 2 => 1.0, 3 => 1.15, _ => 1.4 };
        double constructionFactor = constructionYear < 1945 ? 1.3 : constructionYear < 1990 ? 1.1 : constructionYear >= 2010 ? 0.92 : 1.0;

        AddCoverage(policy, CoverageType.Building, buildingValue, 250m, (95 + (double)buildingValue * 0.00085) * zoneFactor * constructionFactor);
        AddCoverage(policy, CoverageType.Contents, contentsValue, 150m, (40 + (double)contentsValue * 0.0016) * zoneFactor);
    }

    private void AddMotor(Policy policy, PolicyHolder holder)
    {
        var (make, model) = Pick(Vehicles);
        decimal vehicleValue = Between(8, 110) * 500m;
        int powerKw = Between(55, 210);
        int driverAge = Today.Year - holder.DateOfBirth.Year;

        policy.MotorRisk = new MotorRisk
        {
            LicensePlate = $"1-{(char)('A' + _random.Next(26))}{(char)('A' + _random.Next(26))}{(char)('A' + _random.Next(26))}-{_random.Next(100, 1000)}",
            Make = make,
            Model = model,
            FirstRegistrationYear = Between(2008, 2026),
            VehicleValue = vehicleValue,
            PowerKw = powerKw,
            DriverDateOfBirth = holder.DateOfBirth,
            ClaimFreeYears = Between(0, 12)
        };

        double ageFactor = driverAge < 21 ? 1.9 : driverAge < 25 ? 1.55 : driverAge < 30 ? 1.2 : driverAge < 70 ? 1.0 : 1.25;
        double powerFactor = powerKw > 150 ? 1.35 : powerKw > 100 ? 1.15 : 1.0;

        AddCoverage(policy, CoverageType.ThirdPartyLiability, 10_000_000m, 0m, (285 + powerKw * 1.9) * ageFactor * powerFactor);
        if (Chance(0.55))
        {
            AddCoverage(policy, CoverageType.Comprehensive, vehicleValue, Chance(0.5) ? 300m : 500m, (double)vehicleValue * 0.021 * ageFactor);
        }

        if (Chance(0.30))
        {
            AddCoverage(policy, CoverageType.Glass, 1500m, 50m, 38);
        }
    }

    private void AddTravel(Policy policy, PolicyHolder holder)
    {
        bool worldwide = Chance(0.35);
        bool family = Chance(0.30);

        policy.TravelRisk = new TravelRisk
        {
            Scope = worldwide ? TravelScope.Worldwide : TravelScope.Europe,
            PartyType = family ? TravelPartyType.Family : TravelPartyType.Single
        };

        AddCoverage(
            policy, CoverageType.Travel, worldwide ? 100_000m : 50_000m, 100m,
            (family ? 98 : 52) * (worldwide ? 1.45 : 1.0));

        AddInsuredPerson(policy, holder, holder.FirstName, holder.LastName, holder.DateOfBirth, holder.NationalNumber, InsuredRelationship.Self);
        if (!family)
        {
            return;
        }

        var partnerBirth = holder.DateOfBirth.AddDays(_random.Next(-1500, 1500));
        AddInsuredPerson(policy, holder, Pick(FirstNames), holder.LastName, partnerBirth, NationalNumberFor(partnerBirth), InsuredRelationship.Partner);

        for (int child = Between(1, 2); child > 0; child--)
        {
            var childBirth = new DateOnly(2008, 1, 1).AddDays(_random.Next(0, 5000));
            AddInsuredPerson(policy, holder, Pick(FirstNames), holder.LastName, childBirth, null, InsuredRelationship.Child);
        }
    }

    private static void AddInsuredPerson(
        Policy policy, PolicyHolder holder, string firstName, string lastName,
        DateOnly dateOfBirth, string nationalNumber, InsuredRelationship relationship)
    {
        policy.InsuredPersons.Add(new InsuredPerson
        {
            PolicyHolder = holder,
            FirstName = firstName,
            LastName = lastName,
            DateOfBirth = dateOfBirth,
            NationalNumber = nationalNumber,
            Relationship = relationship
        });
    }

    private static void AddCoverage(Policy policy, CoverageType type, decimal limit, decimal deductible, double premium) =>
        policy.Coverages.Add(new Coverage
        {
            Type = type,
            Limit = limit,
            Deductible = deductible,
            Premium = Math.Round((decimal)premium, 2, MidpointRounding.AwayFromZero)
        });

    private static void ApplyPremium(Policy policy)
    {
        decimal net = policy.Coverages.Sum(c => c.Premium);
        decimal discounted = Math.Round(net * (1 - policy.DiscountPercent / 100m), 2, MidpointRounding.AwayFromZero);
        decimal tax = Math.Round(discounted * InsuranceTaxRate, 2, MidpointRounding.AwayFromZero);

        policy.TaxAmount = tax;
        policy.AnnualPremium = discounted + tax;
    }

    private void NumberPolicies()
    {
        foreach (var group in Policies.GroupBy(p => (p.ProductType, p.StartDate.Year)))
        {
            int sequence = 0;
            foreach (var policy in group.OrderBy(p => p.StartDate))
            {
                policy.PolicyNumber = PolicyNumberGenerator.Format(group.Key.ProductType, group.Key.Year, ++sequence);
            }
        }
    }

    private void CreateEndorsements()
    {
        var types = new[]
        {
            (EndorsementType.AddressChange, "Change of correspondence address"),
            (EndorsementType.CoverageChange, "Optional coverage added at the policyholder's request"),
            (EndorsementType.ValueAdjustment, "Insured value indexed"),
            (EndorsementType.PaymentFrequencyChange, "Payment frequency changed"),
            (EndorsementType.VehicleChange, "Vehicle replaced")
        };

        foreach (var policy in Policies.Where(p => p.Status == PolicyStatus.Active && p.StartDate < Today.AddDays(-60)))
        {
            if (!Chance(0.18))
            {
                continue;
            }

            var (type, description) = Pick(types);
            decimal delta = Between(-40, 120);
            var effective = policy.StartDate.AddDays(Between(10, Today.DayNumber - policy.StartDate.DayNumber - 1));

            policy.Endorsements.Add(new PolicyEndorsement
            {
                Type = type,
                EffectiveDate = effective,
                Description = description,
                OldAnnualPremium = policy.AnnualPremium - delta,
                NewAnnualPremium = policy.AnnualPremium,
                PremiumDelta = delta,
                CreatedAt = At(effective.AddDays(-1), 14),
                CreatedBy = "underwriter@scheldt.example"
            });
        }
    }
}
