using ClaimHub.Domain.Enums;

namespace ClaimHub.Domain.Entities;

public class RatingTableEntry
{
    public int Id { get; set; }

    public ProductType ProductType { get; set; }

    // For example "zone", "age", "power".
    public string Factor { get; set; } = string.Empty;

    public string Key { get; set; } = string.Empty;

    public decimal Multiplier { get; set; } = 1m;

    public DateOnly ValidFrom { get; set; }

    public DateOnly? ValidTo { get; set; }

    public bool IsValidOn(DateOnly date) => date >= ValidFrom && (ValidTo is null || date <= ValidTo);
}
