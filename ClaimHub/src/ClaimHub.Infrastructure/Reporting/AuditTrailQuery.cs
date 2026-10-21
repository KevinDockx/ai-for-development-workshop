using ClaimHub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ClaimHub.Infrastructure.Reporting;

public class AuditTrailQuery
{
    public const int DefaultLimit = 20;

    private readonly ClaimHubDbContext _db;

    public AuditTrailQuery(ClaimHubDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<AuditTrailItem>> ForEntityAsync(
        string entityName,
        int entityId,
        CancellationToken cancellationToken,
        int limit = DefaultLimit)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entityName);
        ArgumentOutOfRangeException.ThrowIfLessThan(limit, 1);

        var entries = await _db.AuditEntries.AsNoTracking()
            .Where(a => a.EntityName == entityName && a.EntityId == entityId)
            .OrderByDescending(a => a.Id)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return entries
            .Select(a => new AuditTrailItem(a.Id, a.Timestamp, a.UserName, a.Action.ToString(), a.Details))
            .ToList();
    }
}