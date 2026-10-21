using ClaimHub.Contracts.Claims;
using ClaimHub.Domain.Entities;
using ClaimHub.Domain.Models;

namespace ClaimHub.Api.Features.Claims;

public static class ClaimMappingExtensions
{
    public static ClaimDto ToDto(this Claim claim) => new(
        claim.Id,
        claim.ClaimNumber,
        claim.PolicyId,
        claim.Policy?.PolicyNumber,
        claim.Status,
        claim.DateOfLoss,
        claim.ReportedAt,
        claim.Description,
        claim.IsLateReported,
        claim.AssessedAmount,
        claim.ApprovedAmount,
        claim.PaidAmount);

    public static ClaimStatusHistoryDto ToDto(this ClaimStatusHistory history) =>
        new(history.FromStatus, history.ToStatus, history.ChangedAt, history.ChangedBy, history.Note);

    public static ClaimTimelineEntryDto ToDto(this ClaimTimelineEntry entry) =>
        new(entry.At, entry.Kind.ToString(), entry.Title, entry.Detail);

    public static ClaimDocumentDto ToDto(this ClaimDocument document) =>
        new(document.Id, document.FileName, document.Type, document.SizeBytes, document.UploadedAt);

    public static FraudFlagDto ToDto(this FraudFlag flag) =>
        new(flag.Type, flag.Details, flag.Weight, flag.IsReviewed);

    public static ClaimFinancialsDto ToDto(this ClaimFinancialSummary summary) => new(
        summary.Deductible,
        summary.CoverageLimit,
        summary.RemainingLimit,
        summary.ExpectedPayout,
        summary.Reserve,
        summary.LimitReached);
}
