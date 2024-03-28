using ClaimHub.Contracts.Claims;
using FluentValidation;

namespace ClaimHub.Api.Features.Claims;

public class ReportClaimBodyValidator : AbstractValidator<ReportClaimBody>
{
    public ReportClaimBodyValidator()
    {
        RuleFor(x => x.PolicyId).GreaterThan(0);
        RuleFor(x => x.DateOfLoss)
            .Must(d => d <= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("The date of loss cannot be in the future.");
        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(2000);
        RuleFor(x => x.CoverageType).IsInEnum().When(x => x.CoverageType.HasValue);
    }
}

public class AssessClaimBodyValidator : AbstractValidator<AssessClaimBody>
{
    public AssessClaimBodyValidator()
    {
        RuleFor(x => x.AssessedAmount).GreaterThan(0m).LessThanOrEqualTo(1_000_000m);
        RuleFor(x => x.Note).MaximumLength(500);
    }
}

public class ApproveClaimBodyValidator : AbstractValidator<ApproveClaimBody>
{
    public ApproveClaimBodyValidator()
    {
        RuleFor(x => x.Note).MaximumLength(500);
    }
}

public class RejectClaimBodyValidator : AbstractValidator<RejectClaimBody>
{
    public RejectClaimBodyValidator()
    {
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}

public class WithdrawClaimBodyValidator : AbstractValidator<WithdrawClaimBody>
{
    public WithdrawClaimBodyValidator()
    {
        RuleFor(x => x.Reason).MaximumLength(500);
    }
}

public class ListClaimsParametersValidator : AbstractValidator<ListClaimsParameters>
{
    public ListClaimsParametersValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
