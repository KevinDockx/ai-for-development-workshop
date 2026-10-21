using ClaimHub.Domain.Enums;

namespace ClaimHub.Domain.Entities;

public class InsuredPerson
{
    public int Id { get; set; }

    public int PolicyHolderId { get; set; }

    public virtual PolicyHolder? PolicyHolder { get; set; }

    public int? PolicyId { get; set; }

    public virtual Policy? Policy { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public DateOnly DateOfBirth { get; set; }

    public string? NationalNumber { get; set; }

    public InsuredRelationship Relationship { get; set; } = InsuredRelationship.Self;

    public string FullName => $"{FirstName} {LastName}".Trim();
}
