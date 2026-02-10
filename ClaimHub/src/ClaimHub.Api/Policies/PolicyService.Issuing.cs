using ClaimHub.Contracts.Policies;
using ClaimHub.Domain.Common;
using ClaimHub.Domain.Entities;
using ClaimHub.Domain.Enums;
using ClaimHub.Domain.Models;
using ClaimHub.Domain.Services;
using ClaimHub.Infrastructure.External;

namespace ClaimHub.Api.Policies;

public partial class PolicyService
{
    public async Task<PolicyDto> IssueAsync(IssuePolicyRequest request, string userName, CancellationToken cancellationToken)
    {
        var holder = await RequireHolderAsync(request.PolicyHolderId, cancellationToken);
        var policy = await BuildDraftAsync(request, holder, cancellationToken);
        var quote = await CalculateAsync(policy, holder, cancellationToken);

        ApplyQuote(policy, quote);
        AttachInsuredPersons(policy, holder, request.InsuredPersons);
        _policyValidator.Validate(policy, Today).ThrowIfInvalid();

        policy.PolicyNumber = _numbers.Generate(policy.ProductType, policy.StartDate.Year);
        policy.Activate();
        await AttachInvoicesAsync(policy, quote, cancellationToken);

        await _uow.Policies.AddAsync(policy, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        await AuditAsync(nameof(Policy), policy.Id, AuditAction.Created, userName,
            $"Issued {policy.ProductType} policy {policy.PolicyNumber}. {_explanations.Summarize(_explanations.Explain(quote))}", cancellationToken);

        _logger.LogInformation("Issued policy {PolicyNumber} for policyholder {HolderId}", policy.PolicyNumber, holder.Id);
        await SendConfirmationAsync(policy, holder, cancellationToken);

        var stored = await _uow.Policies.GetWithDetailsAsync(policy.Id, cancellationToken);
        return _mapper.Map<PolicyDto>(stored);
    }

    private void AttachInsuredPersons(Policy policy, PolicyHolder holder, IEnumerable<InsuredPersonInput> inputs)
    {
        var people = inputs.Select(input => new InsuredPerson
        {
            PolicyHolderId = holder.Id,
            FirstName = input.FirstName,
            LastName = input.LastName,
            DateOfBirth = input.DateOfBirth,
            NationalNumber = input.NationalNumber,
            Relationship = input.Relationship
        }).ToList();

        // Travel cover always insures the policyholder; for the other products the list is optional.
        if (policy.ProductType == ProductType.Travel && people.All(p => p.Relationship != InsuredRelationship.Self))
        {
            people.Insert(0, new InsuredPerson
            {
                PolicyHolderId = holder.Id,
                FirstName = holder.FirstName,
                LastName = holder.LastName,
                DateOfBirth = holder.DateOfBirth,
                NationalNumber = holder.NationalNumber,
                Relationship = InsuredRelationship.Self
            });
        }

        foreach (var person in people)
        {
            _insuredValidator.Validate(person, policy, Today).ThrowIfInvalid();
            policy.InsuredPersons.Add(person);
        }
    }

    private async Task AttachInvoicesAsync(Policy policy, PremiumQuote quote, CancellationToken cancellationToken)
    {
        var today = Today;

        for (int i = 0; i < quote.Installments.Count; i++)
        {
            var installment = quote.Installments[i];
            var earliestIssue = installment.DueDate.AddDays(-30);
            var invoice = Invoice.ForInstallment(policy, installment, earliestIssue < today ? today : earliestIssue);
            invoice.InvoiceNumber = await _invoiceNumbers.NextAsync(InvoiceNumberKinds.Invoice, today.Year, cancellationToken);
            invoice.InstallmentCount = quote.Installments.Count;
            policy.Invoices.Add(invoice);
        }
    }

    private Task SendConfirmationAsync(Policy policy, PolicyHolder holder, CancellationToken cancellationToken)
    {
        var message = new EmailMessage(
            holder.Email,
            $"Your policy {policy.PolicyNumber}",
            $"Dear {holder.FullName}, your {policy.ProductType} policy starts on {policy.StartDate:dd/MM/yyyy}. "
            + $"The annual premium is {policy.AnnualPremium:0.00} EUR including tax.");

        return _email.SendAsync(message, cancellationToken);
    }
}
