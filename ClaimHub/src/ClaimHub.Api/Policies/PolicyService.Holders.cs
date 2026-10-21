using ClaimHub.Contracts.Common;
using ClaimHub.Contracts.Policies;
using ClaimHub.Contracts.PolicyHolders;
using ClaimHub.Domain.Common;
using ClaimHub.Domain.Entities;
using ClaimHub.Domain.Enums;
using ClaimHub.Infrastructure.External;
using Microsoft.EntityFrameworkCore;

namespace ClaimHub.Api.Policies;

public partial class PolicyService
{
    public async Task<PagedResult<PolicyHolderSummaryDto>> ListHoldersAsync(
        PolicyHolderQuery query,
        CancellationToken cancellationToken)
    {
        var holders = _uow.Repository<PolicyHolder>().Query().Where(h => !h.IsDeleted);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            string term = query.Search.Trim().ToLower();
            holders = holders.Where(h =>
                h.LastName.ToLower().Contains(term)
                || h.FirstName.ToLower().Contains(term)
                || h.Email.ToLower().Contains(term));
        }

        int total = await holders.CountAsync(cancellationToken);
        var page = await holders
            .OrderBy(h => h.LastName)
            .ThenBy(h => h.FirstName)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<PolicyHolderSummaryDto>(
            _mapper.Map<List<PolicyHolderSummaryDto>>(page), query.Page, query.PageSize, total);
    }

    public async Task<PolicyHolderDto> GetHolderAsync(int id, CancellationToken cancellationToken)
    {
        var holder = await _uow.Repository<PolicyHolder>().Query()
            .Include(h => h.Policies)
            .FirstOrDefaultAsync(h => h.Id == id && !h.IsDeleted, cancellationToken);

        return holder == null ? null : _mapper.Map<PolicyHolderDto>(holder);
    }

    public async Task<PolicyHolderDto> CreateHolderAsync(
        CreatePolicyHolderRequest request,
        string userName,
        CancellationToken cancellationToken)
    {
        var holder = _mapper.Map<PolicyHolder>(request);
        holder.Role = UserRole.PolicyHolder;
        holder.CustomerSince = Today;
        holder.CreatedAt = Now;

        _holderValidator.Validate(holder, Today).ThrowIfInvalid();
        await EnsureIbanAcceptedAsync(holder.Iban, cancellationToken);
        await EnsureHolderIsUniqueAsync(holder, cancellationToken);
        await EnsureBrokerExistsAsync(holder.BrokerId, cancellationToken);

        await _uow.Repository<PolicyHolder>().AddAsync(holder, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        await AuditAsync(nameof(PolicyHolder), holder.Id, AuditAction.Created, userName, "Policyholder registered", cancellationToken);

        _logger.LogInformation(
            "Registered policyholder {HolderId} {Name} with IBAN {Iban} and national number {NationalNumber}",
            holder.Id, holder.FullName, holder.Iban, holder.NationalNumber);

        await SendWelcomeMailAsync(holder, cancellationToken);
        return _mapper.Map<PolicyHolderDto>(holder);
    }

    public async Task<PolicyHolder> UpdateHolderAsync(
        int id,
        PolicyHolder incoming,
        string userName,
        CancellationToken cancellationToken)
    {
        var holder = await _uow.Repository<PolicyHolder>().GetByIdAsync(id, cancellationToken);
        if (holder == null || holder.IsDeleted)
        {
            return null;
        }

        holder.FirstName = incoming.FirstName;
        holder.LastName = incoming.LastName;
        holder.Email = incoming.Email;
        holder.Phone = incoming.Phone;
        holder.DateOfBirth = incoming.DateOfBirth;
        holder.NationalNumber = incoming.NationalNumber;
        holder.Iban = incoming.Iban;
        holder.Street = incoming.Street;
        holder.PostalCode = incoming.PostalCode;
        holder.City = incoming.City;
        holder.CustomerSince = incoming.CustomerSince;
        holder.BrokerId = incoming.BrokerId;
        holder.Role = incoming.Role;
        holder.IsVip = incoming.IsVip;
        holder.CreditLimit = incoming.CreditLimit;
        holder.UpdatedAt = Now;

        _holderValidator.Validate(holder, Today).ThrowIfInvalid();
        await EnsureBrokerExistsAsync(holder.BrokerId, cancellationToken);

        await _uow.SaveChangesAsync(cancellationToken);
        await AuditAsync(nameof(PolicyHolder), holder.Id, AuditAction.Updated, userName, "Policyholder details changed", cancellationToken);

        _logger.LogInformation(
            "Updated policyholder {HolderId}: IBAN {Iban}, national number {NationalNumber}, VIP {IsVip}",
            holder.Id, holder.Iban, holder.NationalNumber, holder.IsVip);

        return holder;
    }

    public async Task<List<PolicyListItemDto>> GetPoliciesForHolderAsync(int policyHolderId, CancellationToken cancellationToken)
    {
        var holder = await _uow.Repository<PolicyHolder>().GetByIdAsync(policyHolderId, cancellationToken);
        if (holder == null || holder.IsDeleted)
        {
            return null;
        }

        var policies = await _uow.Policies.GetForPolicyHolderAsync(policyHolderId, cancellationToken);
        var items = _mapper.Map<List<PolicyListItemDto>>(policies);
        items.ForEach(i => i.PolicyHolderName = holder.FullName);
        return items;
    }

    private async Task EnsureIbanAcceptedAsync(string iban, CancellationToken cancellationToken)
    {
        IbanCheckResult result = await _ibanCheck.CheckAsync(iban, cancellationToken);
        if (!result.IsValid)
        {
            throw new DomainRuleException($"The IBAN was rejected: {result.Reason}");
        }
    }

    private async Task EnsureHolderIsUniqueAsync(PolicyHolder holder, CancellationToken cancellationToken)
    {
        var repository = _uow.Repository<PolicyHolder>();
        if (await repository.AnyAsync(h => h.Email == holder.Email && !h.IsDeleted, cancellationToken))
        {
            throw new DomainRuleException("A policyholder with this e-mail address already exists.");
        }

        if (await repository.AnyAsync(h => h.NationalNumber == holder.NationalNumber && !h.IsDeleted, cancellationToken))
        {
            throw new DomainRuleException("A policyholder with this national number already exists.");
        }
    }

    private async Task EnsureBrokerExistsAsync(int? brokerId, CancellationToken cancellationToken)
    {
        if (brokerId is not int id)
        {
            return;
        }

        if (!await _uow.Repository<Broker>().AnyAsync(b => b.Id == id && b.IsActive, cancellationToken))
        {
            throw new DomainRuleException($"Broker {id} does not exist or is no longer active.");
        }
    }

    private Task SendWelcomeMailAsync(PolicyHolder holder, CancellationToken cancellationToken)
    {
        var message = new EmailMessage(
            holder.Email,
            "Welcome to Scheldt Assurance",
            $"Dear {holder.FullName}, your customer file has been created. You can now request a quote for home, motor or travel insurance.");

        return _email.SendAsync(message, cancellationToken);
    }
}
