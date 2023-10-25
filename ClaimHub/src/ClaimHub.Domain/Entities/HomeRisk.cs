namespace ClaimHub.Domain.Entities;

public class HomeRisk
{
    public int Id { get; set; }

    public int PolicyId { get; set; }

    public virtual Policy? Policy { get; set; }

    public string Street { get; set; } = string.Empty;

    public string PostalCode { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    // 1 (lowest risk) to 4 (highest risk)
    public int PostcodeZone { get; set; } = 1;

    public decimal BuildingValue { get; set; }

    public decimal ContentsValue { get; set; }

    public int ConstructionYear { get; set; }

    public bool IsOwnerOccupied { get; set; } = true;

    public int BuildingAgeIn(int year) => Math.Max(0, year - ConstructionYear);

    public bool HasValidZone() => PostcodeZone is >= 1 and <= 4;
}
