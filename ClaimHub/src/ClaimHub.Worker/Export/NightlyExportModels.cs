namespace ClaimHub.Worker.Export;

public class NightlyExport
{
    public DateTimeOffset GeneratedAt { get; set; }

    public List<PolicyHolderExport> PolicyHolders { get; set; } = new();

    public List<PolicyExport> Policies { get; set; } = new();

    public List<ClaimExport> Claims { get; set; } = new();
}

public class PolicyHolderExport
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string DateOfBirth { get; set; } = string.Empty;

    public string NationalNumber { get; set; } = string.Empty;

    public string Iban { get; set; } = string.Empty;

    public string Street { get; set; } = string.Empty;

    public string PostalCode { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string CustomerSince { get; set; } = string.Empty;

    public bool IsVip { get; set; }
}

public class PolicyExport
{
    public int Id { get; set; }

    public string PolicyNumber { get; set; } = string.Empty;

    public string ProductType { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public int PolicyHolderId { get; set; }

    public string StartDate { get; set; } = string.Empty;

    public string EndDate { get; set; } = string.Empty;

    public decimal AnnualPremium { get; set; }

    public string PaymentFrequency { get; set; } = string.Empty;
}

public class ClaimExport
{
    public int Id { get; set; }

    public string ClaimNumber { get; set; } = string.Empty;

    public int PolicyId { get; set; }

    public string Status { get; set; } = string.Empty;

    public string DateOfLoss { get; set; } = string.Empty;

    public DateTimeOffset ReportedAt { get; set; }

    public string Description { get; set; } = string.Empty;

    public decimal? AssessedAmount { get; set; }

    public decimal? ApprovedAmount { get; set; }

    public decimal PaidAmount { get; set; }

    public int? AssignedHandlerId { get; set; }

    public DateTimeOffset? EscalatedAt { get; set; }
}