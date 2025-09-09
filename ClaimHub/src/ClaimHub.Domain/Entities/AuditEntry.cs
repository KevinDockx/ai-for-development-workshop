using ClaimHub.Domain.Enums;

namespace ClaimHub.Domain.Entities;

public class AuditEntry
{
    public long Id { get; set; }

    public string EntityName { get; set; } = string.Empty;

    public int EntityId { get; set; }

    public AuditAction Action { get; set; }

    public int? UserId { get; set; }

    public string UserName { get; set; } = string.Empty;

    public DateTimeOffset Timestamp { get; set; }

    public string? Details { get; set; }

    public static AuditEntry For(string entityName, int entityId, AuditAction action, string userName, DateTimeOffset at, string? details = null) =>
        new()
        {
            EntityName = entityName,
            EntityId = entityId,
            Action = action,
            UserName = userName,
            Timestamp = at,
            Details = details
        };
}
