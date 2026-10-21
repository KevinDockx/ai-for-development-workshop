using System.ComponentModel.DataAnnotations;
using ClaimHub.Domain.Enums;

namespace ClaimHub.Contracts.PolicyHolders;

public class PolicyHolderSummaryDto
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public DateOnly CustomerSince { get; set; }

    public int? BrokerId { get; set; }
}

public class PolicyHolderDto : PolicyHolderSummaryDto
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public DateOnly DateOfBirth { get; set; }

    public string NationalNumber { get; set; } = string.Empty;

    public string Iban { get; set; } = string.Empty;

    public string Street { get; set; } = string.Empty;

    public string PostalCode { get; set; } = string.Empty;

    public bool IsVip { get; set; }

    public decimal CreditLimit { get; set; }

    public int PolicyCount { get; set; }
}

public class CreatePolicyHolderRequest
{
    [Required, StringLength(80)]
    public string FirstName { get; set; } = string.Empty;

    [Required, StringLength(80)]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(200)]
    public string Email { get; set; } = string.Empty;

    [Phone, StringLength(30)]
    public string? Phone { get; set; }

    [Required]
    public DateOnly DateOfBirth { get; set; }

    [Required, RegularExpression(@"^\d{2}\.\d{2}\.\d{2}-\d{3}\.\d{2}$", ErrorMessage = "Use the format YY.MM.DD-XXX.CC.")]
    public string NationalNumber { get; set; } = string.Empty;

    [Required, StringLength(34, MinimumLength = 15)]
    public string Iban { get; set; } = string.Empty;

    [Required, StringLength(120)]
    public string Street { get; set; } = string.Empty;

    [Required, StringLength(4, MinimumLength = 4)]
    public string PostalCode { get; set; } = string.Empty;

    [Required, StringLength(80)]
    public string City { get; set; } = string.Empty;

    public int? BrokerId { get; set; }
}

public class PolicyHolderQuery
{
    public string? Search { get; set; }

    [Range(1, 10_000)]
    public int Page { get; set; } = 1;

    [Range(1, 200)]
    public int PageSize { get; set; } = 25;
}
