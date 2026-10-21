using ClaimHub.Domain.Entities;
using ClaimHub.Domain.Enums;

namespace ClaimHub.Infrastructure.Repositories;

public interface IPolicyRepository : IRepository<Policy>
{
    Task<Policy> GetWithDetailsAsync(int id, CancellationToken cancellationToken = default);

    Task<Policy> GetByNumberAsync(string policyNumber, CancellationToken cancellationToken = default);

    Task<List<Policy>> GetForPolicyHolderAsync(int policyHolderId, CancellationToken cancellationToken = default);

    Task<List<Policy>> GetForBrokerAsync(int brokerId, CancellationToken cancellationToken = default);

    Task<List<Policy>> GetExpiringBetweenAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default);

    Task<(List<Policy> Items, int Total)> SearchAsync(
        PolicyStatus? status,
        ProductType? product,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
