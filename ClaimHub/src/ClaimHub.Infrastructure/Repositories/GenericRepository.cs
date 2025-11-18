using System.Linq.Expressions;
using ClaimHub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ClaimHub.Infrastructure.Repositories;

public class GenericRepository<T> : IRepository<T> where T : class
{
    protected readonly ClaimHubDbContext Context;
    protected readonly DbSet<T> Set;

    public GenericRepository(ClaimHubDbContext context)
    {
        Context = context;
        Set = context.Set<T>();
    }

    public virtual async Task<T> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await Set.FindAsync(new object[] { id }, cancellationToken);

    public virtual Task<List<T>> ListAsync(CancellationToken cancellationToken = default) =>
        Set.ToListAsync(cancellationToken);

    public virtual Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default) =>
        Set.Where(predicate).ToListAsync(cancellationToken);

    public virtual Task<int> CountAsync(Expression<Func<T, bool>> predicate = null, CancellationToken cancellationToken = default) =>
        predicate == null ? Set.CountAsync(cancellationToken) : Set.CountAsync(predicate, cancellationToken);

    public virtual Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default) =>
        Set.AnyAsync(predicate, cancellationToken);

    public virtual async Task AddAsync(T entity, CancellationToken cancellationToken = default) =>
        await Set.AddAsync(entity, cancellationToken);

    public virtual void Update(T entity) => Set.Update(entity);

    public virtual void Remove(T entity) => Set.Remove(entity);

    public virtual IQueryable<T> Query() => Set.AsQueryable();
}
