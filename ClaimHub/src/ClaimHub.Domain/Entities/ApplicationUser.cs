using ClaimHub.Domain.Enums;

namespace ClaimHub.Domain.Entities;

public class ApplicationUser
{
    public int Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    public bool IsActive { get; set; } = true;

    public int? PolicyHolderId { get; set; }

    public virtual PolicyHolder? PolicyHolder { get; set; }

    public int? BrokerId { get; set; }

    public virtual Broker? Broker { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? LastLoginAt { get; set; }

    public bool IsStaff => Role != UserRole.PolicyHolder && Role != UserRole.Broker;

    public bool CanAssessClaims => Role is UserRole.ClaimsHandler or UserRole.SeniorClaimsHandler or UserRole.Admin;

    public void RecordLogin(DateTimeOffset at) => LastLoginAt = at;
}
