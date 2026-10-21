using ClaimHub.Domain.Entities;

namespace ClaimHub.Domain.Legacy;

public class LegacyCustomerRecord
{
    public string CustomerNo { get; set; } = string.Empty;

    public string ClientName { get; set; } = string.Empty;

    public string InsuredName { get; set; } = string.Empty;

    public string ContractNumber { get; set; } = string.Empty;
}

public class LegacyLossRecord
{
    public string ContractNumber { get; set; } = string.Empty;

    public DateOnly LossDate { get; set; }

    public string IncidentReference { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal ClaimedAmount { get; set; }
}

public static class LegacyNamingExtensions
{
    public static string GetContractNumber(this Policy policy) => policy.PolicyNumber;

    public static LegacyCustomerRecord ToCustomerRecord(this PolicyHolder holder, Policy policy) =>
        new()
        {
            CustomerNo = holder.Id.ToString("D6"),
            ClientName = holder.FullName,
            InsuredName = policy.InsuredPersons.FirstOrDefault()?.FullName ?? holder.FullName,
            ContractNumber = policy.GetContractNumber()
        };

    public static DateOnly GetLossDate(this Claim claim) => claim.DateOfLoss;

    public static string GetIncidentReference(this Claim claim) => claim.ClaimNumber;
}
