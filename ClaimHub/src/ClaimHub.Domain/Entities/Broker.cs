namespace ClaimHub.Domain.Entities;

public class Broker
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string RegistrationNumber { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public decimal CommissionRate { get; set; } = 0.12m;

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; }

    public virtual ICollection<Policy> Policies { get; set; } = new List<Policy>();

    public virtual ICollection<PolicyHolder> PolicyHolders { get; set; } = new List<PolicyHolder>();

    public virtual ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();

    public decimal CommissionFor(decimal premium) => Math.Round(premium * CommissionRate, 2, MidpointRounding.AwayFromZero);
}
