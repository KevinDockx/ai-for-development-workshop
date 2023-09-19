using ClaimHub.Domain.Entities;
using ClaimHub.Domain.Enums;

namespace ClaimHub.Domain.Models;

public class FraudAssessment
{
    public int ClaimId { get; set; }

    public IList<FraudFlag> Flags { get; set; } = new List<FraudFlag>();

    public int Score => Flags.Sum(f => f.Weight);

    public bool HasFlags => Flags.Count > 0;

    public FraudRiskLevel RiskLevel => Score switch
    {
        0 => FraudRiskLevel.None,
        < 30 => FraudRiskLevel.Low,
        < 60 => FraudRiskLevel.Medium,
        _ => FraudRiskLevel.High
    };
}
