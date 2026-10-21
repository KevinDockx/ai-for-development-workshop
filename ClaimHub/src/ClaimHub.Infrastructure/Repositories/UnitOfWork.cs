using ClaimHub.Infrastructure.Data;

namespace ClaimHub.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ClaimHubDbContext _context;
    private readonly Dictionary<Type, object> _repositories = new();
    private IPolicyRepository _policies;

    public UnitOfWork(ClaimHubDbContext context)
    {
        _context = context;
    }

    public IPolicyRepository Policies => _policies ??= new PolicyRepository(_context);

    public IRepository<T> Repository<T>() where T : class
    {
        if (!_repositories.TryGetValue(typeof(T), out var repository))
        {
            repository = new GenericRepository<T>(_context);
            _repositories[typeof(T)] = repository;
        }

        return (IRepository<T>)repository;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);

    public void Dispose() => _context.Dispose();
}
