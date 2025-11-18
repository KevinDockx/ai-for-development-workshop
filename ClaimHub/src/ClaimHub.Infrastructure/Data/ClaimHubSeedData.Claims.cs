using ClaimHub.Domain.Entities;
using ClaimHub.Domain.Enums;
using ClaimHub.Domain.Services;

namespace ClaimHub.Infrastructure.Data;

public partial class ClaimHubSeedData
{
    // Status, number of claims, and how far back the loss may lie. Together they add up to ClaimCount.
    private static readonly (ClaimStatus Status, int Count, int MaxAgeDays)[] ClaimPlan =
    {
        (ClaimStatus.Reported, 20, 25),
        (ClaimStatus.UnderAssessment, 25, 60),
        (ClaimStatus.Approved, 15, 90),
        (ClaimStatus.Rejected, 15, 200),
        (ClaimStatus.PaidOut, 25, 250),
        (ClaimStatus.Closed, 35, 500),
        (ClaimStatus.Withdrawn, 15, 200)
    };

    private static readonly Dictionary<CoverageType, string[]> LossDescriptions = new()
    {
        [CoverageType.Building] = new[]
        {
            "Water damage after a burst pipe in the bathroom wall.",
            "Storm damage to roof tiles and gutter.",
            "Fire damage to the garage after an electrical fault."
        },
        [CoverageType.Contents] = new[]
        {
            "Burglary: television and laptop taken from the living room.",
            "Contents damaged by water leaking from the apartment above.",
            "Smoke damage to furniture after a kitchen fire."
        },
        [CoverageType.ThirdPartyLiability] = new[]
        {
            "Collision at a roundabout; the other driver's car was damaged.",
            "Rear-end collision in traffic with minor injuries reported.",
            "Parked car damaged while reversing out of a driveway."
        },
        [CoverageType.Comprehensive] = new[]
        {
            "Vehicle damaged by hail during a storm.",
            "Theft of the vehicle from a public car park.",
            "Single-vehicle collision with a fence."
        },
        [CoverageType.Glass] = new[]
        {
            "Windscreen cracked by a stone on the motorway.",
            "Side window broken during an attempted break-in."
        },
        [CoverageType.Travel] = new[]
        {
            "Luggage delayed for four days; essential purchases made.",
            "Medical treatment abroad after a fall while hiking.",
            "Cancelled trip because of a family emergency."
        }
    };

    private void CreateClaims()
    {
        var handlers = Users.Where(u => u.Role == UserRole.ClaimsHandler).ToList();
        var seniors = Users.Where(u => u.Role == UserRole.SeniorClaimsHandler).ToList();
        var loginPolicies = Policies
            .Where(p => p.PolicyHolder == PolicyHolders[0] || p.PolicyHolder == PolicyHolders[1])
            .ToList();

        var claims = new List<Claim>();
        foreach (var (status, count, maxAgeDays) in ClaimPlan)
        {
            for (int i = 0; i < count; i++)
            {
                claims.Add(BuildClaim(status, maxAgeDays, loginPolicies, handlers, seniors));
            }
        }

        var perYear = new Dictionary<int, int>();
        foreach (var claim in claims.OrderBy(c => c.ReportedAt))
        {
            int year = claim.ReportedAt.Year;
            perYear[year] = perYear.GetValueOrDefault(year) + 1;
            claim.ClaimNumber = ClaimNumberGenerator.Format(year, perYear[year]);

            AddDocuments(claim);
            AddPayout(claim);
            Claims.Add(claim);
        }

        RaiseFraudFlags();
    }

    private Claim BuildClaim(ClaimStatus status, int maxAgeDays, List<Policy> loginPolicies, List<ApplicationUser> handlers, List<ApplicationUser> seniors)
    {
        Policy policy = null;
        DateOnly dateOfLoss = default;
        for (int attempt = 0; attempt < 500 && policy == null; attempt++)
        {
            var candidate = Chance(0.10) ? Pick(loginPolicies) : Pick(Policies);
            if (TryPickDateOfLoss(candidate, maxAgeDays, out dateOfLoss))
            {
                policy = candidate;
            }
        }

        if (policy == null)
        {
            throw new InvalidOperationException($"No policy qualifies for a {status} claim in the seed data.");
        }

        var coverage = Pick(policy.Coverages.ToList());
        int lagDays = Chance(0.15) ? Between(31, 45) : Between(0, 10);
        var reportedOn = Today < dateOfLoss.AddDays(lagDays) ? Today : dateOfLoss.AddDays(lagDays);
        var reportedAt = NotAfterNow(At(reportedOn, Between(8, 17), Between(0, 59)));
        decimal assessed = AssessedAmountFor(coverage);

        var claim = new Claim
        {
            Policy = policy,
            Coverage = coverage,
            Status = status,
            DateOfLoss = dateOfLoss,
            ReportedAt = reportedAt,
            CreatedAt = reportedAt,
            IsLateReported = reportedOn.DayNumber - dateOfLoss.DayNumber > 30,
            Description = Pick(LossDescriptions[coverage.Type])
        };

        string reporter = policy.PolicyHolder.Email;
        AddStep(claim, ClaimStatus.Reported, ClaimStatus.Reported, reportedAt, reporter, "Claim reported");

        if (status == ClaimStatus.Reported)
        {
            if (Today.DayNumber - reportedOn.DayNumber >= 8 && Chance(0.6))
            {
                claim.EscalatedAt = At(reportedOn.AddDays(8), 7);
            }

            return claim;
        }

        if (status == ClaimStatus.Withdrawn && Chance(0.4))
        {
            AddStep(claim, ClaimStatus.Reported, ClaimStatus.Withdrawn, reportedAt.AddDays(Between(1, 6)), reporter, "Withdrawn by the policyholder");
            claim.UpdatedAt = claim.StatusHistory.Last().ChangedAt;
            return claim;
        }

        var handler = Pick(handlers);
        var approver = assessed > 10000m ? Pick(seniors) : handler;
        var assessmentStart = reportedAt.AddDays(Between(1, 4)).AddHours(Between(0, 5));

        claim.AssignedHandler = handler;
        claim.FirstAssessmentAt = NotAfterNow(assessmentStart);
        AddStep(claim, ClaimStatus.Reported, ClaimStatus.UnderAssessment, assessmentStart, handler.Email, "Assessment started");

        if (status == ClaimStatus.UnderAssessment)
        {
            claim.AssessedAmount = Chance(0.5) ? assessed : null;
            claim.UpdatedAt = claim.FirstAssessmentAt;
            return claim;
        }

        if (status == ClaimStatus.Withdrawn)
        {
            AddStep(claim, ClaimStatus.UnderAssessment, ClaimStatus.Withdrawn, assessmentStart.AddDays(Between(2, 12)), reporter, "Withdrawn by the policyholder");
            claim.UpdatedAt = claim.StatusHistory.Last().ChangedAt;
            return claim;
        }

        var decidedAt = assessmentStart.AddDays(Between(1, 10));
        claim.AssessedAmount = assessed;

        bool rejected = status == ClaimStatus.Rejected || (status == ClaimStatus.Closed && Chance(0.35));
        if (rejected)
        {
            AddStep(claim, ClaimStatus.UnderAssessment, ClaimStatus.Rejected, decidedAt, approver.Email, "Loss is not covered under the policy terms");
            if (status == ClaimStatus.Closed)
            {
                AddStep(claim, ClaimStatus.Rejected, ClaimStatus.Closed, decidedAt.AddDays(Between(5, 30)), handler.Email, "Closed after rejection");
            }

            claim.UpdatedAt = claim.StatusHistory.Last().ChangedAt;
            return claim;
        }

        claim.ApprovedAmount = Math.Min(assessed - coverage.Deductible, coverage.Limit);
        AddStep(claim, ClaimStatus.UnderAssessment, ClaimStatus.Approved, decidedAt, approver.Email, "Approved");

        if (status != ClaimStatus.Approved)
        {
            var paidAt = decidedAt.AddDays(Between(1, 7));
            claim.PaidAmount = claim.ApprovedAmount.Value;
            claim.PaidAt = NotAfterNow(paidAt);
            AddStep(claim, ClaimStatus.Approved, ClaimStatus.PaidOut, paidAt, handler.Email, "Payout released");

            if (status == ClaimStatus.Closed)
            {
                AddStep(claim, ClaimStatus.PaidOut, ClaimStatus.Closed, paidAt.AddDays(Between(3, 30)), handler.Email, "Closed after payout");
            }
        }

        claim.UpdatedAt = claim.StatusHistory.Last().ChangedAt;
        return claim;
    }

    private bool TryPickDateOfLoss(Policy policy, int maxAgeDays, out DateOnly dateOfLoss)
    {
        dateOfLoss = default;
        if (policy.Status == PolicyStatus.Quoted)
        {
            return false;
        }

        var earliest = Max(policy.StartDate, Today.AddDays(-maxAgeDays));
        var latest = Min(policy.EndDate.AddDays(-1), Today.AddDays(-4));
        if (policy.CancelledOn.HasValue)
        {
            latest = Min(latest, policy.CancelledOn.Value.AddDays(-1));
        }

        if (earliest > latest)
        {
            return false;
        }

        if (Chance(0.08) && policy.StartDate >= earliest && policy.StartDate.AddDays(14) <= latest)
        {
            dateOfLoss = policy.StartDate.AddDays(Between(0, 14));
            return true;
        }

        dateOfLoss = earliest.AddDays(_random.Next(0, latest.DayNumber - earliest.DayNumber + 1));
        return true;
    }

    private decimal AssessedAmountFor(Coverage coverage)
    {
        if (coverage.Limit <= 100_000m && Chance(0.12))
        {
            return Math.Round(coverage.Limit * 0.85m / 50m) * 50m;
        }

        decimal ceiling = coverage.Type switch
        {
            CoverageType.Glass => 600m,
            CoverageType.Travel => 4000m,
            CoverageType.Contents => 8000m,
            CoverageType.Comprehensive => 12000m,
            CoverageType.ThirdPartyLiability => 20000m,
            _ => 25000m
        };

        int low = (int)((coverage.Deductible + 100m) / 50m) + 1;
        int high = Math.Max(low + 1, (int)(Math.Min(ceiling, coverage.Limit) / 50m));
        return Between(low, high) * 50m;
    }

    private static void AddStep(Claim claim, ClaimStatus from, ClaimStatus to, DateTimeOffset at, string changedBy, string note) =>
        claim.StatusHistory.Add(new ClaimStatusHistory
        {
            FromStatus = from,
            ToStatus = to,
            ChangedAt = NotAfterNow(at),
            ChangedBy = changedBy,
            Note = note
        });

    private void AddDocuments(Claim claim)
    {
        string uploader = claim.Policy.PolicyHolder.Email;
        var product = claim.Policy.ProductType;

        if (Chance(0.85))
        {
            AddDocument(claim, DocumentType.ClaimForm, "claim-form.pdf", "application/pdf", uploader);
        }

        if (product != ProductType.Travel && Chance(0.55))
        {
            for (int n = Between(1, 3); n > 0; n--)
            {
                AddDocument(claim, DocumentType.Photo, $"photo-{n}.jpg", "image/jpeg", uploader);
            }
        }

        if (claim.AssessedAmount.HasValue && Chance(0.5))
        {
            AddDocument(claim, DocumentType.Invoice, "repair-invoice.pdf", "application/pdf", uploader);
        }

        if (product == ProductType.Motor && Chance(0.2))
        {
            AddDocument(claim, DocumentType.PoliceReport, "police-report.pdf", "application/pdf", uploader);
        }

        if (product == ProductType.Travel && Chance(0.3))
        {
            AddDocument(claim, DocumentType.MedicalCertificate, "medical-certificate.pdf", "application/pdf", uploader);
        }
    }

    private void AddDocument(Claim claim, DocumentType type, string fileName, string contentType, string uploadedBy) =>
        claim.Documents.Add(new ClaimDocument
        {
            Type = type,
            FileName = fileName,
            StoredPath = Path.Combine(claim.ClaimNumber, fileName),
            ContentType = contentType,
            SizeBytes = PlaceholderContent(claim.ClaimNumber, fileName).Length,
            UploadedAt = NotAfterNow(claim.ReportedAt.AddHours(Between(1, 48))),
            UploadedBy = uploadedBy
        });

    private void AddPayout(Claim claim)
    {
        if (claim.PaidAmount <= 0m || claim.PaidAt == null)
        {
            return;
        }

        Payments.Add(new Payment
        {
            Direction = PaymentDirection.Outgoing,
            Claim = claim,
            Amount = claim.PaidAmount,
            Method = PaymentMethod.BankTransfer,
            Status = PaymentStatus.Completed,
            GatewayReference = Reference($"payout|{claim.ClaimNumber}"),
            IdempotencyKey = $"payout-{claim.ClaimNumber}",
            CreatedAt = claim.PaidAt.Value,
            CompletedAt = claim.PaidAt
        });
    }

    private void RaiseFraudFlags()
    {
        string reviewer = Users.First(u => u.Role == UserRole.SeniorClaimsHandler).Email;

        foreach (var claim in Claims)
        {
            var policy = claim.Policy;
            var raisedAt = NotAfterNow(claim.ReportedAt.AddMinutes(5));

            if (claim.DateOfLoss.DayNumber - policy.StartDate.DayNumber <= 14)
            {
                AddFlag(claim, FraudFlagType.LossShortlyAfterStart, 20, "Date of loss is within 14 days of the policy start.", raisedAt, reviewer);
            }

            if (claim.AssessedAmount > claim.Coverage.Limit * 0.8m)
            {
                AddFlag(claim, FraudFlagType.HighAmountComparedToLimit, 30, "Assessed amount is above 80% of the coverage limit.", raisedAt, reviewer);
            }

            int inLastYear = Claims.Count(other =>
                other.Policy.PolicyHolder == policy.PolicyHolder
                && other.DateOfLoss <= claim.DateOfLoss
                && other.DateOfLoss > claim.DateOfLoss.AddYears(-1));
            if (inLastYear > 2)
            {
                AddFlag(claim, FraudFlagType.FrequentClaims, 25, $"{inLastYear} claims by this policyholder in the last 12 months.", raisedAt, reviewer);
            }
        }
    }

    private void AddFlag(Claim claim, FraudFlagType type, int weight, string details, DateTimeOffset raisedAt, string reviewer)
    {
        bool reviewed = claim.Status != ClaimStatus.Reported && Chance(0.4);
        claim.FraudFlags.Add(new FraudFlag
        {
            Type = type,
            Weight = weight,
            Details = details,
            RaisedAt = raisedAt,
            IsReviewed = reviewed,
            ReviewedBy = reviewed ? reviewer : null
        });
    }

    private static DateOnly Max(DateOnly left, DateOnly right) => left > right ? left : right;

    private static DateOnly Min(DateOnly left, DateOnly right) => left < right ? left : right;
}
