using ClaimHub.Contracts.Common;
using ClaimHub.Contracts.Policies;
using ClaimHub.Domain.Entities;
using ClaimHub.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ClaimHub.Api.Policies;

public partial class PolicyService
{
    public async Task<PolicyDto> GetPolicyAsync(int id, CancellationToken cancellationToken)
    {
        var policy = await _uow.Policies.GetWithDetailsAsync(id, cancellationToken);
        return policy == null ? null : _mapper.Map<PolicyDto>(policy);
    }

    public async Task<PolicyDto> GetPolicyByNumberAsync(string policyNumber, CancellationToken cancellationToken)
    {
        var policy = await _uow.Policies.GetByNumberAsync(policyNumber, cancellationToken);
        return policy == null ? null : _mapper.Map<PolicyDto>(policy);
    }

    public async Task<PagedResult<PolicyListItemDto>> SearchPoliciesAsync(
        PolicySearchQuery query,
        CancellationToken cancellationToken)
    {
        var (items, total) = await _uow.Policies.SearchAsync(
            query.Status, query.Product, query.Page, query.PageSize, cancellationToken);

        return new PagedResult<PolicyListItemDto>(
            _mapper.Map<List<PolicyListItemDto>>(items), query.Page, query.PageSize, total);
    }

    public async Task<List<PolicyListItemDto>> GetExpiringAsync(int days, CancellationToken cancellationToken)
    {
        var from = Today;
        var policies = await _uow.Policies.GetExpiringBetweenAsync(from, from.AddDays(days), cancellationToken);
        return _mapper.Map<List<PolicyListItemDto>>(policies);
    }

    public async Task<List<PolicyListItemDto>> GetPoliciesForBrokerAsync(int brokerId, CancellationToken cancellationToken)
    {
        var policies = await _uow.Policies.GetForBrokerAsync(brokerId, cancellationToken);
        return _mapper.Map<List<PolicyListItemDto>>(policies);
    }

    public async Task<decimal> GetOutstandingBalanceAsync(int policyId, CancellationToken cancellationToken)
    {
        var invoices = await _uow.Repository<Invoice>().Query()
            .Where(i => i.PolicyId == policyId && i.Status != InvoiceStatus.Cancelled)
            .Include(i => i.Payments)
            .ToListAsync(cancellationToken);

        return invoices.Sum(i => i.OutstandingAmount);
    }
}
