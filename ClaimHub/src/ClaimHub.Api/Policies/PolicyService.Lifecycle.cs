using ClaimHub.Contracts.Policies;
using ClaimHub.Domain.Common;
using ClaimHub.Domain.Entities;
using ClaimHub.Domain.Enums;
using ClaimHub.Domain.Models;
using ClaimHub.Domain.Rules;
using ClaimHub.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace ClaimHub.Api.Policies;

public partial class PolicyService
{
    public async Task<EndorsementResultDto> EndorseAsync(
        int policyId,
        EndorsePolicyRequest request,
        string userName,
        CancellationToken cancellationToken)
    {
        var policy = await _uow.Policies.GetWithDetailsAsync(policyId, cancellationToken);
        if (policy == null)
        {
            return null;
        }

        if (policy.Status != PolicyStatus.Active)
        {
            throw new DomainRuleException($"A policy in status {policy.Status} cannot be endorsed.");
        }

        var holder = await RequireHolderAsync(policy.PolicyHolderId, cancellationToken);
        await ApplyEndorsementChangesAsync(policy, request, cancellationToken);

        var quote = await CalculateAsync(policy, holder, cancellationToken);
        var premiumResult = _changes.DescribeEndorsement(policy, quote.AnnualPremium, request.EffectiveDate, request.Type).Premium;
        var endorsement = _endorsements.ToEndorsement(
            policy, premiumResult, request.Type, request.Description, userName, Now);

        _endorsementValidator.Validate(policy, endorsement).ThrowIfInvalid();

        ApplyQuote(policy, quote);
        policy.UpdatedAt = Now;
        policy.Endorsements.Add(endorsement);

        if (premiumResult.AdditionalPremium > 0m)
        {
            var today = Today;
            var extra = new Installment(1, today.AddDays(30), premiumResult.AdditionalPremium);
            var invoice = Invoice.ForInstallment(policy, extra, today);
            invoice.InvoiceNumber = await _invoiceNumbers.NextAsync(InvoiceNumberKinds.Invoice, today.Year, cancellationToken);
            policy.Invoices.Add(invoice);
        }

        await _uow.SaveChangesAsync(cancellationToken);
        await AuditAsync(nameof(Policy), policy.Id, AuditAction.Updated, userName,
            $"Endorsement {request.Type} effective {request.EffectiveDate:yyyy-MM-dd}", cancellationToken);

        var result = _mapper.Map<EndorsementResultDto>(premiumResult);
        result.PolicyId = policy.Id;
        result.EndorsementId = endorsement.Id;
        return result;
    }

    public async Task<CancellationResultDto> CancelAsync(
        int policyId,
        CancelPolicyRequest request,
        string userName,
        CancellationToken cancellationToken)
    {
        var policy = await _uow.Policies.GetWithDetailsAsync(policyId, cancellationToken);
        if (policy == null)
        {
            return null;
        }

        var effectiveOn = request.EffectiveDate ?? Today;

        // The refund rules look at the claims of the current term, so make sure they are loaded.
        await _uow.Repository<Claim>().Query()
            .Where(c => c.PolicyId == policyId && !c.IsDeleted)
            .LoadAsync(cancellationToken);
        var invoices = await _uow.Repository<Invoice>().FindAsync(i => i.PolicyId == policyId, cancellationToken);

        var refund = _changes.DescribeCancellation(policy, effectiveOn, invoices).Refund;
        policy.Cancel(effectiveOn, request.Reason);
        policy.UpdatedAt = Now;

        foreach (var open in invoices.Where(i => i.Status is InvoiceStatus.Issued or InvoiceStatus.Overdue && i.DueDate > effectiveOn))
        {
            open.Cancel();
        }

        string creditNoteNumber = await IssueRefundCreditNoteAsync(invoices, refund, cancellationToken);

        await _uow.SaveChangesAsync(cancellationToken);
        await AuditAsync(nameof(Policy), policy.Id, AuditAction.StatusChanged, userName,
            $"Cancelled effective {effectiveOn:yyyy-MM-dd}: {request.Reason}", cancellationToken);

        var result = _mapper.Map<CancellationResultDto>(refund);
        result.PolicyId = policy.Id;
        result.Status = policy.Status;
        result.CreditNoteNumber = creditNoteNumber;
        return result;
    }

    public async Task<PolicyDto> RenewAsync(
        int policyId,
        RenewPolicyRequest request,
        string userName,
        CancellationToken cancellationToken)
    {
        var current = await _uow.Policies.GetWithDetailsAsync(policyId, cancellationToken);
        if (current == null)
        {
            return null;
        }

        if (current.Status != PolicyStatus.Active)
        {
            throw new DomainRuleException($"A policy in status {current.Status} cannot be renewed.");
        }

        var holder = await RequireHolderAsync(current.PolicyHolderId, cancellationToken);
        var term = _terms.Renew(new PolicyTerm(current.StartDate, current.EndDate));

        var renewal = new Policy
        {
            ProductType = current.ProductType,
            PolicyHolderId = current.PolicyHolderId,
            BrokerId = current.BrokerId,
            RenewedFromPolicyId = current.Id,
            StartDate = term.Start,
            EndDate = term.End,
            PaymentFrequency = request.NewPaymentFrequency ?? current.PaymentFrequency,
            CreatedAt = Now
        };

        CopyRisk(current, renewal, request.ClaimFreeYears);
        AddCoverages(renewal, current.Coverages.Where(c => c.IsActive).Select(c => c.Type));

        var quote = await CalculateAsync(renewal, holder, cancellationToken);
        ApplyQuote(renewal, quote);
        CopyInsuredPersons(current, renewal);

        _policyValidator.Validate(renewal, Today).ThrowIfInvalid();
        renewal.PolicyNumber = _numbers.Generate(renewal.ProductType, renewal.StartDate.Year);
        renewal.Activate();
        await AttachInvoicesAsync(renewal, quote, cancellationToken);

        current.MarkRenewed();
        current.UpdatedAt = Now;

        await _uow.Policies.AddAsync(renewal, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        await AuditAsync(nameof(Policy), renewal.Id, AuditAction.Created, userName,
            $"Renewal of {current.PolicyNumber} as {renewal.PolicyNumber}", cancellationToken);

        var stored = await _uow.Policies.GetWithDetailsAsync(renewal.Id, cancellationToken);
        return _mapper.Map<PolicyDto>(stored);
    }

    private async Task ApplyEndorsementChangesAsync(Policy policy, EndorsePolicyRequest request, CancellationToken cancellationToken)
    {
        if (request.NewPaymentFrequency.HasValue)
        {
            policy.PaymentFrequency = request.NewPaymentFrequency.Value;
        }

        if (policy.HomeRisk is { } home)
        {
            home.BuildingValue = request.NewBuildingValue ?? home.BuildingValue;
            home.ContentsValue = request.NewContentsValue ?? home.ContentsValue;
            home.Street = request.NewStreet ?? home.Street;
            home.City = request.NewCity ?? home.City;

            if (request.NewPostalCode != null && request.NewPostalCode != home.PostalCode)
            {
                home.PostalCode = request.NewPostalCode;
                var suggestions = await _addresses.LookupByPostalCodeAsync(home.PostalCode, cancellationToken);
                home.PostcodeZone = suggestions.Count > 0 ? suggestions[0].RiskZone : home.PostcodeZone;
            }
        }

        if (policy.MotorRisk is { } motor && request.NewVehicleValue.HasValue)
        {
            motor.VehicleValue = request.NewVehicleValue.Value;
        }

        if (request.NewCoverages != null)
        {
            ProductCoverageRules.Validate(policy.ProductType, request.NewCoverages).ThrowIfInvalid();
            SwitchCoverages(policy, request.NewCoverages);
        }

        RefreshLimits(policy);
    }

    private static void SwitchCoverages(Policy policy, IReadOnlyCollection<CoverageType> wanted)
    {
        foreach (var coverage in policy.Coverages.Where(c => c.IsActive && !wanted.Contains(c.Type)))
        {
            coverage.IsActive = false;
        }

        foreach (var type in wanted.Where(t => policy.FindCoverage(t) is null))
        {
            var dormant = policy.Coverages.FirstOrDefault(c => c.Type == type);
            if (dormant != null)
            {
                dormant.IsActive = true;
            }
            else
            {
                AddCoverages(policy, new[] { type });
            }
        }
    }

    private static void RefreshLimits(Policy policy)
    {
        foreach (var coverage in policy.Coverages.Where(c => c.IsActive))
        {
            var (limit, _) = LimitsFor(policy, coverage.Type);
            coverage.Limit = limit;
        }
    }

    private static void CopyRisk(Policy source, Policy target, int? claimFreeYears)
    {
        if (source.HomeRisk is { } home)
        {
            target.HomeRisk = new HomeRisk
            {
                Street = home.Street,
                PostalCode = home.PostalCode,
                City = home.City,
                PostcodeZone = home.PostcodeZone,
                BuildingValue = home.BuildingValue,
                ContentsValue = home.ContentsValue,
                ConstructionYear = home.ConstructionYear,
                IsOwnerOccupied = home.IsOwnerOccupied
            };
        }

        if (source.MotorRisk is { } motor)
        {
            target.MotorRisk = new MotorRisk
            {
                LicensePlate = motor.LicensePlate,
                Make = motor.Make,
                Model = motor.Model,
                FirstRegistrationYear = motor.FirstRegistrationYear,
                VehicleValue = motor.VehicleValue,
                PowerKw = motor.PowerKw,
                DriverDateOfBirth = motor.DriverDateOfBirth,
                ClaimFreeYears = claimFreeYears ?? motor.ClaimFreeYears + 1
            };
        }

        if (source.TravelRisk is { } travel)
        {
            target.TravelRisk = new TravelRisk { Scope = travel.Scope, PartyType = travel.PartyType };
        }
    }

    private static void CopyInsuredPersons(Policy source, Policy target)
    {
        foreach (var person in source.InsuredPersons)
        {
            target.InsuredPersons.Add(new InsuredPerson
            {
                PolicyHolderId = person.PolicyHolderId,
                FirstName = person.FirstName,
                LastName = person.LastName,
                DateOfBirth = person.DateOfBirth,
                NationalNumber = person.NationalNumber,
                Relationship = person.Relationship
            });
        }
    }

    private async Task<string> IssueRefundCreditNoteAsync(
        IReadOnlyCollection<Invoice> invoices,
        CancellationRefundResult refund,
        CancellationToken cancellationToken)
    {
        if (!refund.HasRefund)
        {
            return null;
        }

        var paid = invoices
            .Where(i => i.Status == InvoiceStatus.Paid && !i.IsCreditNote)
            .OrderByDescending(i => i.IssuedOn)
            .FirstOrDefault();
        if (paid == null)
        {
            return null;
        }

        var existingNotes = invoices.Where(i => i.IsCreditNote).ToList();
        decimal amount = Math.Min(refund.RefundAmount, _creditNotes.CreditableAmount(paid, existingNotes));
        if (amount <= 0m)
        {
            return null;
        }

        var today = Today;
        var note = _creditNotes.Issue(paid, amount, existingNotes, today);
        note.InvoiceNumber = await _invoiceNumbers.NextAsync(InvoiceNumberKinds.CreditNote, today.Year, cancellationToken);
        await _uow.Repository<Invoice>().AddAsync(note, cancellationToken);
        return note.InvoiceNumber;
    }
}
