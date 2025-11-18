using System.Security.Cryptography;
using System.Text;
using ClaimHub.Domain.Entities;
using ClaimHub.Domain.Enums;
using ClaimHub.Infrastructure.External;

namespace ClaimHub.Infrastructure.Data;

/// <summary>
/// Builds the demo data set in memory. The generator uses a fixed random seed and a fixed "today",
/// so a fresh database always receives the same rows.
/// </summary>
public partial class ClaimHubSeedData
{
    public const string SeedPassword = "Passw0rd!";
    public const int PolicyHolderCount = 200;
    public const int PolicyCount = 350;
    public const int ClaimCount = 150;
    private const int QuotedPolicyCount = 12;
    private const int RandomSeed = 20240611;

    public static readonly DateOnly Today = new(2026, 10, 1);
    public static readonly DateTimeOffset Now = new(2026, 10, 1, 8, 0, 0, TimeSpan.Zero);

    private static readonly string[] FirstNames =
    {
        "An", "Jonas", "Lotte", "Pieter", "Sofie", "Wouter", "Eline", "Bram", "Katrien", "Thomas",
        "Marie", "Dieter", "Hannelore", "Koen", "Fien", "Stijn", "Charlotte", "Joris", "Nele", "Maarten",
        "Silke", "Tim", "Griet", "Jan", "Evi", "Tom", "Lien", "Geert", "Ine", "Kris"
    };

    private static readonly string[] LastNames =
    {
        "Peeters", "Maes", "Janssens", "Willems", "Claes", "Goossens", "Wouters", "Mertens", "Dewulf", "Vermeulen",
        "Hermans", "Aerts", "Jacobs", "Smets", "Lambrecht", "Desmet", "Verbeke", "Coppens", "Michiels", "Bogaert",
        "Cools", "Segers", "Dierickx", "Wauters", "Hendrickx"
    };

    private static readonly string[] StreetNames =
    {
        "Proefstraat", "Voorbeeldlaan", "Demostraat", "Testlaan", "Fictiestraat",
        "Exempelplein", "Schijnstraat", "Modelweg", "Monsterstraat", "Oefenlaan"
    };

    private readonly Random _random = new(RandomSeed);
    private readonly Dictionary<Invoice, Invoice> _creditedInvoices = new();

    private ClaimHubSeedData()
    {
    }

    public List<Broker> Brokers { get; } = new();

    public List<ApplicationUser> Users { get; } = new();

    public List<PolicyHolder> PolicyHolders { get; } = new();

    public List<Policy> Policies { get; } = new();

    public List<Claim> Claims { get; } = new();

    public List<Invoice> Invoices { get; } = new();

    public List<Payment> Payments { get; } = new();

    public List<RatingTableEntry> RatingTable { get; } = new();

    public List<AuditEntry> AuditEntries { get; } = new();

    public static ClaimHubSeedData Generate()
    {
        var seed = new ClaimHubSeedData();
        seed.CreateBrokers();
        seed.CreatePolicyHolders();
        seed.CreateUsers();
        seed.CreatePolicies();
        seed.CreateEndorsements();
        seed.CreateClaims();
        seed.CreateInvoices();
        seed.CreateRatingTable();
        return seed;
    }

    public static byte[] PlaceholderContent(string claimNumber, string fileName) =>
        Encoding.UTF8.GetBytes($"Placeholder content for {fileName} on {claimNumber}. Synthetic demo document.");

    public static string HashPassword(string password) =>
        Convert.ToHexStringLower(MD5.HashData(Encoding.UTF8.GetBytes(password)));

    /// <summary>Fills in values that only exist once the first save assigned database ids.</summary>
    public void CompleteAfterInsert()
    {
        foreach (var (creditNote, original) in _creditedInvoices)
        {
            creditNote.CreditedInvoiceId = original.Id;
        }

        foreach (var claim in Claims.Where(c => c.StatusHistory.Count > 1).Take(60))
        {
            foreach (var step in claim.StatusHistory.Where(h => h.FromStatus != h.ToStatus))
            {
                AuditEntries.Add(AuditEntry.For(
                    "Claim", claim.Id, AuditAction.StatusChanged, step.ChangedBy, step.ChangedAt,
                    $"{step.FromStatus} -> {step.ToStatus}"));
            }
        }

        foreach (var policy in Policies.Where(p => p.Status == PolicyStatus.Cancelled))
        {
            AuditEntries.Add(AuditEntry.For(
                "Policy", policy.Id, AuditAction.StatusChanged, "underwriter@scheldt.example",
                At(policy.CancelledOn ?? policy.StartDate, 9), policy.CancellationReason));
        }
    }

    private void CreateBrokers()
    {
        string[] names =
        {
            "Demo Verzekeringskantoor Antwerpen", "Voorbeeld Makelaars Gent",
            "Proef Assuradeurs Leuven", "Fictief Verzekeringen Brugge"
        };
        decimal[] commission = { 0.10m, 0.12m, 0.15m, 0.11m };

        for (int i = 0; i < names.Length; i++)
        {
            Brokers.Add(new Broker
            {
                Name = names[i],
                RegistrationNumber = $"DEMO-BRK-{i + 1:D4}",
                Email = $"info@demo-broker-{i + 1}.example",
                Phone = $"+32 400 00 {i + 1:D4}",
                CommissionRate = commission[i],
                CreatedAt = At(new DateOnly(2019, 3, 1).AddDays(i * 40), 9)
            });
        }
    }

    private void CreatePolicyHolders()
    {
        for (int i = 0; i < PolicyHolderCount; i++)
        {
            string first = i < 2 ? FirstNames[i] : Pick(FirstNames);
            string last = i < 2 ? LastNames[i] : Pick(LastNames);
            string email = i switch
            {
                0 => "an.peeters@mail.example",
                1 => "jonas.maes@mail.example",
                _ => $"{first}.{last}.{i + 1}@mail.example".ToLowerInvariant()
            };

            var dateOfBirth = i switch
            {
                0 => new DateOnly(1985, 4, 17),
                1 => new DateOnly(1991, 11, 2),
                _ => new DateOnly(1950, 1, 1).AddDays(_random.Next(0, 19000))
            };

            var town = Pick(FakeAddressLookup.Municipalities);
            var customerSince = new DateOnly(2010, 1, 1).AddDays(_random.Next(0, 5400));
            bool vip = i > 1 && Chance(0.05);

            PolicyHolders.Add(new PolicyHolder
            {
                FirstName = first,
                LastName = last,
                Email = email,
                Phone = $"+32 400 {_random.Next(0, 100):D2} {_random.Next(0, 10000):D4}",
                DateOfBirth = dateOfBirth,
                NationalNumber = NationalNumberFor(dateOfBirth),
                Iban = IbanFor(),
                Street = $"{Pick(StreetNames)} {_random.Next(1, 150)}",
                PostalCode = town.PostalCode,
                City = town.City,
                CustomerSince = customerSince,
                IsVip = vip,
                CreditLimit = vip ? 10000m : Chance(0.3) ? Between(1, 10) * 500m : 0m,
                Broker = BrokerFor(i),
                CreatedAt = At(customerSince, 9)
            });
        }
    }

    private Broker BrokerFor(int holderIndex)
    {
        // The two login holders sit with different brokers so cross-portfolio access can be demonstrated.
        if (holderIndex < 2)
        {
            return Brokers[holderIndex];
        }

        double roll = _random.NextDouble();
        return roll switch
        {
            < 0.25 => Brokers[0],
            < 0.45 => Brokers[1],
            < 0.55 => Brokers[2],
            < 0.62 => Brokers[3],
            _ => null
        };
    }

    private void CreateUsers()
    {
        AddUser("admin@scheldt.example", "Admin Scheldt", UserRole.Admin);
        AddUser("underwriter@scheldt.example", "Una Underwriter", UserRole.Underwriter);
        AddUser("handler@scheldt.example", "Hans Handler", UserRole.ClaimsHandler);
        AddUser("senior@scheldt.example", "Sara Senior", UserRole.SeniorClaimsHandler);
        AddUser("broker1@scheldt.example", "Broker One", UserRole.Broker, broker: Brokers[0]);
        AddUser("broker2@scheldt.example", "Broker Two", UserRole.Broker, broker: Brokers[1]);
        AddUser("an.peeters@mail.example", "An Peeters", UserRole.PolicyHolder, holder: PolicyHolders[0]);
        AddUser("jonas.maes@mail.example", "Jonas Maes", UserRole.PolicyHolder, holder: PolicyHolders[1]);
    }

    private void AddUser(string email, string displayName, UserRole role, Broker broker = null, PolicyHolder holder = null)
    {
        Users.Add(new ApplicationUser
        {
            Email = email,
            DisplayName = displayName,
            PasswordHash = HashPassword(SeedPassword),
            Role = role,
            Broker = broker,
            PolicyHolder = holder,
            CreatedAt = At(new DateOnly(2023, 1, 9), 8),
            LastLoginAt = At(Today.AddDays(-Between(1, 20)), 9)
        });
    }

    private void CreateRatingTable()
    {
        // The 2024-2025 table was replaced by the 2026 revision; both stay in the table.
        AddRatingSet(new DateOnly(2024, 1, 1), new DateOnly(2025, 12, 31), zoneThree: 1.10m, zoneFour: 1.35m);
        AddRatingSet(new DateOnly(2026, 1, 1), null, zoneThree: 1.15m, zoneFour: 1.40m);
    }

    private void AddRatingSet(DateOnly from, DateOnly? to, decimal zoneThree, decimal zoneFour)
    {
        AddRating(ProductType.Home, "zone", "1", 0.90m, from, to);
        AddRating(ProductType.Home, "zone", "2", 1.00m, from, to);
        AddRating(ProductType.Home, "zone", "3", zoneThree, from, to);
        AddRating(ProductType.Home, "zone", "4", zoneFour, from, to);
        AddRating(ProductType.Home, "construction", "pre1945", 1.30m, from, to);
        AddRating(ProductType.Home, "construction", "pre1990", 1.10m, from, to);
        AddRating(ProductType.Home, "construction", "standard", 1.00m, from, to);
        AddRating(ProductType.Home, "construction", "modern", 0.92m, from, to);
        AddRating(ProductType.Motor, "age", "under21", 1.90m, from, to);
        AddRating(ProductType.Motor, "age", "under25", 1.55m, from, to);
        AddRating(ProductType.Motor, "age", "under30", 1.20m, from, to);
        AddRating(ProductType.Motor, "age", "standard", 1.00m, from, to);
        AddRating(ProductType.Motor, "age", "senior", 1.25m, from, to);
        AddRating(ProductType.Travel, "scope", "Europe", 1.00m, from, to);
        AddRating(ProductType.Travel, "scope", "Worldwide", 1.45m, from, to);
    }

    private void AddRating(ProductType product, string factor, string key, decimal multiplier, DateOnly from, DateOnly? to) =>
        RatingTable.Add(new RatingTableEntry
        {
            ProductType = product,
            Factor = factor,
            Key = key,
            Multiplier = multiplier,
            ValidFrom = from,
            ValidTo = to
        });

    private string NationalNumberFor(DateOnly dateOfBirth)
    {
        int serial = _random.Next(1, 999);
        long digits = long.Parse($"{dateOfBirth:yyMMdd}{serial:D3}");
        long checkBase = dateOfBirth.Year >= 2000 ? 2_000_000_000L + digits : digits;
        long check = 97 - checkBase % 97;

        return $"{dateOfBirth:yy.MM.dd}-{serial:D3}.{check:D2}";
    }

    // Bank code 999 is not assigned to a real bank, so these account numbers are well-formed but fictional.
    private string IbanFor()
    {
        long account = 9_990_000_000L + _random.Next(0, 10_000_000);
        long accountCheck = account % 97 == 0 ? 97 : account % 97;
        string bban = $"{account:D10}{accountCheck:D2}";
        long remainder = long.Parse(bban + "111400") % 97;

        return $"BE{98 - remainder:D2}{bban}";
    }

    private T Pick<T>(IReadOnlyList<T> items) => items[_random.Next(items.Count)];

    private bool Chance(double probability) => _random.NextDouble() < probability;

    private int Between(int min, int max) => _random.Next(min, max + 1);

    private static DateTimeOffset At(DateOnly date, int hour, int minute = 0) =>
        new(date.ToDateTime(new TimeOnly(hour, minute)), TimeSpan.Zero);

    private static DateTimeOffset NotAfterNow(DateTimeOffset value) => value > Now ? Now : value;

    private static string Reference(string source) =>
        "GW-" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(source)), 0, 6);
}
