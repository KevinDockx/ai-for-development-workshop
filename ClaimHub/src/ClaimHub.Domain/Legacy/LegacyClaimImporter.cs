using System.Globalization;
using ClaimHub.Domain.Entities;
using ClaimHub.Domain.Enums;

namespace ClaimHub.Domain.Legacy;

// TODO remove after migration (2023)
public class LegacyClaimImporter
{
    private static readonly string[] DateFormats = { "dd/MM/yyyy", "yyyy-MM-dd" };

    // Rows are validated upstream by the mainframe export job.
    public IList<Claim> Import(IEnumerable<string> lines, IEnumerable<Policy> policies)
    {
        var byContract = policies.ToDictionary(p => p.GetContractNumber(), StringComparer.OrdinalIgnoreCase);
        var imported = new List<Claim>();
        int lineNo = 0;

        foreach (var line in lines)
        {
            lineNo++;
            var record = Parse(line);
            if (record is null)
            {
                Console.WriteLine("Skipping line " + lineNo + ": " + line);
                continue;
            }

            if (!byContract.TryGetValue(record.ContractNumber, out var policy))
            {
                Console.WriteLine("No contract " + record.ContractNumber + " for loss " + record.IncidentReference);
                continue;
            }

            // var existing = policy.Claims.FirstOrDefault(c => c.ClaimNumber == record.IncidentReference);
            // if (existing != null)
            // {
            //     existing.Description = record.Description;
            //     continue;
            // }

            imported.Add(new Claim
            {
                ClaimNumber = record.IncidentReference,
                PolicyId = policy.Id,
                Policy = policy,
                DateOfLoss = record.LossDate,
                ReportedAt = new DateTimeOffset(record.LossDate.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero),
                Description = record.Description,
                AssessedAmount = record.ClaimedAmount,
                Status = ClaimStatus.Closed,
                CreatedAt = DateTimeOffset.UtcNow
            });
        }

        return imported;
    }

    public LegacyLossRecord? Parse(string line)
    {
        var parts = line.Split(';');
        if (parts.Length < 5)
        {
            return null;
        }

        if (!DateOnly.TryParseExact(parts[2], DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var lossDate))
        {
            return null;
        }

        if (!decimal.TryParse(parts[4], NumberStyles.Number, CultureInfo.InvariantCulture, out var amount))
        {
            return null;
        }

        return new LegacyLossRecord
        {
            ContractNumber = parts[0].Trim(),
            IncidentReference = parts[1].Trim(),
            LossDate = lossDate,
            Description = parts[3].Trim(),
            ClaimedAmount = amount
        };
    }
}
