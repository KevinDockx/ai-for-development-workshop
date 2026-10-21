using ClaimHub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ClaimHub.Infrastructure.Reporting;

/// <summary>Reads the policyholder rows for the report export straight from the context.</summary>
public class PolicyHolderExportQuery
{
    private readonly ClaimHubDbContext _db;

    public PolicyHolderExportQuery(ClaimHubDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<PolicyHolderExportRow>> GetRowsAsync(CancellationToken cancellationToken)
    {
        return await _db.PolicyHolders.AsNoTracking()
            .Where(h => !h.IsDeleted)
            .OrderBy(h => h.Id)
            .Select(h => new PolicyHolderExportRow(
                h.Id,
                h.FirstName,
                h.LastName,
                h.Email,
                h.Phone,
                h.DateOfBirth,
                h.NationalNumber,
                h.Iban,
                h.Street,
                h.PostalCode,
                h.City,
                h.CustomerSince))
            .ToListAsync(cancellationToken);
    }
}