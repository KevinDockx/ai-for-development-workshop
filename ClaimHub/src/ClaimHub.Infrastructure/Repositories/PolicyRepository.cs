using ClaimHub.Domain.Entities;
using ClaimHub.Domain.Enums;
using ClaimHub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ClaimHub.Infrastructure.Repositories;

public class PolicyRepository : GenericRepository<Policy>, IPolicyRepository
{
    public PolicyRepository(ClaimHubDbContext context)
        : base(context)
    {
    }

    public async Task<Policy> GetWithDetailsAsync(int id, CancellationToken cancellationToken = default) =>
        await WithDetails().FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, cancellationToken);

    public Task<Policy> GetByNumberAsync(string policyNumber, CancellationToken cancellationToken = default) =>
        WithDetails().FirstOrDefaultAsync(p => p.PolicyNumber == policyNumber && !p.IsDeleted, cancellationToken);

    public Task<List<Policy>> GetForPolicyHolderAsync(int policyHolderId, CancellationToken cancellationToken = default) =>
        Set.Include(p => p.Coverages)
            .Where(p => p.PolicyHolderId == policyHolderId && !p.IsDeleted)
            .OrderByDescending(p => p.StartDate)
            .ToListAsync(cancellationToken);

    public Task<List<Policy>> GetForBrokerAsync(int brokerId, CancellationToken cancellationToken = default) =>
        Set.Include(p => p.PolicyHolder)
            .Where(p => p.BrokerId == brokerId && !p.IsDeleted)
            .OrderByDescending(p => p.StartDate)
            .ToListAsync(cancellationToken);

    public Task<List<Policy>> GetExpiringBetweenAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default) =>
        Set.Include(p => p.Coverages)
            .Where(p => p.Status == PolicyStatus.Active && p.EndDate >= from && p.EndDate < to && !p.IsDeleted)
            .OrderBy(p => p.EndDate)
            .ToListAsync(cancellationToken);

    public async Task<(List<Policy> Items, int Total)> SearchAsync(
        PolicyStatus? status,
        ProductType? product,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = Set.Include(p => p.PolicyHolder).Where(p => !p.IsDeleted);
        if (status.HasValue)
        {
            query = query.Where(p => p.Status == status.Value);
        }

        if (product.HasValue)
        {
            query = query.Where(p => p.ProductType == product.Value);
        }

        int total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(p => p.StartDate)
            .ThenBy(p => p.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    private IQueryable<Policy> WithDetails() =>
        Set.Include(p => p.PolicyHolder)
            .Include(p => p.Broker)
            .Include(p => p.Coverages)
            .Include(p => p.HomeRisk)
            .Include(p => p.MotorRisk)
            .Include(p => p.TravelRisk)
            .Include(p => p.InsuredPersons)
            .Include(p => p.Endorsements)
            .AsSplitQuery();
}
