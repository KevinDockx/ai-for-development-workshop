using ClaimHub.Domain.Enums;

namespace ClaimHub.Domain.Entities;

public class TravelRisk
{
    public int Id { get; set; }

    public int PolicyId { get; set; }

    public virtual Policy? Policy { get; set; }

    public TravelScope Scope { get; set; } = TravelScope.Europe;

    public TravelPartyType PartyType { get; set; } = TravelPartyType.Single;
}
