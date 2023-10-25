using System.Text.Json.Serialization;
using ClaimHub.Domain.Common;
using ClaimHub.Domain.Enums;

namespace ClaimHub.Domain.Entities;

public class PolicyHolder : EntityBase, IAuditable, ISoftDeletable
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public DateOnly DateOfBirth { get; set; }

    // Format YY.MM.DD-XXX.CC
    public string NationalNumber { get; set; } = string.Empty;

    public string Iban { get; set; } = string.Empty;

    public string Street { get; set; } = string.Empty;

    public string PostalCode { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public DateOnly CustomerSince { get; set; }

    public UserRole Role { get; set; } = UserRole.PolicyHolder;

    public bool IsVip { get; set; }

    public decimal CreditLimit { get; set; }

    public int? BrokerId { get; set; }

    [JsonIgnore]
    public virtual Broker? Broker { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public DateTimeOffset? DeletedAt { get; set; }

    public string? DeletedBy { get; set; }

    [JsonIgnore]
    public virtual ICollection<Policy> Policies { get; set; } = new List<Policy>();

    [JsonIgnore]
    public virtual ICollection<InsuredPerson> InsuredPersons { get; set; } = new List<InsuredPerson>();

    public string FullName => $"{FirstName} {LastName}".Trim();

    public int FullYearsAsCustomer(DateOnly on)
    {
        if (on < CustomerSince)
        {
            return 0;
        }

        var years = on.Year - CustomerSince.Year;
        if (on < CustomerSince.AddYears(years))
        {
            years--;
        }

        return years;
    }

    public int ActivePolicyCount(DateOnly on) =>
        Policies.Count(p => !p.IsDeleted && p.Status == PolicyStatus.Active && p.StartDate <= on && on < p.EndDate);

    public void SoftDelete(string deletedBy, DateTimeOffset at)
    {
        IsDeleted = true;
        DeletedAt = at;
        DeletedBy = deletedBy;
    }
}
