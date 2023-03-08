namespace ClaimHub.Domain.Common;

public abstract class EntityBase
{
    public int Id { get; set; }
}

public interface IAuditable
{
    DateTimeOffset CreatedAt { get; set; }

    DateTimeOffset? UpdatedAt { get; set; }
}

public interface ISoftDeletable
{
    bool IsDeleted { get; set; }

    DateTimeOffset? DeletedAt { get; set; }

    string? DeletedBy { get; set; }
}
