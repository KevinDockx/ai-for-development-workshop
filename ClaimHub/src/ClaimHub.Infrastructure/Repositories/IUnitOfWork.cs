namespace ClaimHub.Infrastructure.Repositories;

public interface IUnitOfWork : IDisposable
{
    IPolicyRepository Policies { get; }

    IRepository<T> Repository<T>() where T : class;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
