using AutoMapper;
using ClaimHub.Domain.Common;
using ClaimHub.Domain.Entities;
using ClaimHub.Domain.Enums;
using ClaimHub.Domain.Rules;
using ClaimHub.Domain.Services;
using ClaimHub.Infrastructure.External;
using ClaimHub.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ClaimHub.Api.Policies;

/// <summary>
/// Application service for policyholders and policies: quoting, issuing, endorsing, cancelling and renewing.
/// The implementation is split over several files by topic; this file holds the shared plumbing.
/// </summary>
public partial class PolicyService
{
    private static readonly SemaphoreSlim RatingTableLock = new(1, 1);

    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ILogger<PolicyService> _logger;
    private readonly PremiumCalculator _premiums;
    private readonly PolicyNumberGenerator _numbers;
    private readonly PolicyValidator _policyValidator;
    private readonly PolicyHolderValidator _holderValidator;
    private readonly InsuredPersonValidator _insuredValidator;
    private readonly EndorsementValidator _endorsementValidator;
    private readonly EndorsementPremiumCalculator _endorsements;
    private readonly PolicyChangeDecisionService _changes;
    private readonly PremiumExplanationService _explanations;
    private readonly CreditNoteService _creditNotes;
    private readonly PolicyTermCalculator _terms;
    private readonly DriverAgeCalculator _driverAges;
    private readonly IIbanCheckService _ibanCheck;
    private readonly IAddressLookup _addresses;
    private readonly IEmailSender _email;
    private readonly IInvoiceNumberSequence _invoiceNumbers;

    public PolicyService(
        IUnitOfWork uow,
        IMapper mapper,
        ILogger<PolicyService> logger,
        PremiumCalculator premiums,
        PolicyNumberGenerator numbers,
        PolicyValidator policyValidator,
        PolicyHolderValidator holderValidator,
        InsuredPersonValidator insuredValidator,
        EndorsementValidator endorsementValidator,
        EndorsementPremiumCalculator endorsements,
        PolicyChangeDecisionService changes,
        PremiumExplanationService explanations,
        CreditNoteService creditNotes,
        PolicyTermCalculator terms,
        DriverAgeCalculator driverAges,
        IIbanCheckService ibanCheck,
        IAddressLookup addresses,
        IEmailSender email,
        IInvoiceNumberSequence invoiceNumbers)
    {
        _invoiceNumbers = invoiceNumbers;
        _uow = uow;
        _mapper = mapper;
        _logger = logger;
        _premiums = premiums;
        _numbers = numbers;
        _policyValidator = policyValidator;
        _holderValidator = holderValidator;
        _insuredValidator = insuredValidator;
        _endorsementValidator = endorsementValidator;
        _endorsements = endorsements;
        _changes = changes;
        _explanations = explanations;
        _creditNotes = creditNotes;
        _terms = terms;
        _driverAges = driverAges;
        _ibanCheck = ibanCheck;
        _addresses = addresses;
        _email = email;
    }

    private static DateOnly Today => BelgianTime.Today();

    private static DateTimeOffset Now => DateTimeOffset.UtcNow;

    private async Task<PolicyHolder> RequireHolderAsync(int policyHolderId, CancellationToken cancellationToken)
    {
        var holder = await _uow.Repository<PolicyHolder>().GetByIdAsync(policyHolderId, cancellationToken);
        if (holder == null || holder.IsDeleted)
        {
            throw new DomainRuleException($"Policyholder {policyHolderId} does not exist.");
        }

        return holder;
    }

    private async Task EnsureRatingTableAsync(CancellationToken cancellationToken)
    {
        if (RatingTableCache.Count > 0)
        {
            return;
        }

        await RatingTableLock.WaitAsync(cancellationToken);
        try
        {
            if (RatingTableCache.Count == 0)
            {
                var entries = await _uow.Repository<RatingTableEntry>().ListAsync(cancellationToken);
                RatingTableCache.Load(entries, Today);
            }
        }
        finally
        {
            RatingTableLock.Release();
        }
    }

    private async Task AuditAsync(
        string entityName,
        int entityId,
        AuditAction action,
        string userName,
        string details,
        CancellationToken cancellationToken)
    {
        var entry = AuditEntry.For(entityName, entityId, action, userName, Now, details);
        await _uow.Repository<AuditEntry>().AddAsync(entry, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
