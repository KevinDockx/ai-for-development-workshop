using System.Globalization;
using ClaimHub.Domain.Enums;

namespace ClaimHub.Admin.Infrastructure;

public static class Ui
{
    private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

    public static string Money(decimal? amount) =>
        amount is null ? "-" : "€ " + amount.Value.ToString("N2", Culture);

    public static string Date(DateOnly? date) =>
        date is null ? "-" : date.Value.ToString("dd MMM yyyy", Culture);

    public static string When(DateTimeOffset? timestamp) =>
        timestamp is null ? "-" : timestamp.Value.ToLocalTime().ToString("dd MMM yyyy HH:mm", Culture);

    public static string ClaimBadge(ClaimStatus status) => status switch
    {
        ClaimStatus.Reported => "text-bg-secondary",
        ClaimStatus.UnderAssessment => "text-bg-info",
        ClaimStatus.Approved => "text-bg-success",
        ClaimStatus.Rejected => "text-bg-danger",
        ClaimStatus.PaidOut => "text-bg-primary",
        ClaimStatus.Closed => "text-bg-dark",
        _ => "text-bg-light border"
    };

    public static string PolicyBadge(PolicyStatus status) => status switch
    {
        PolicyStatus.Active => "text-bg-success",
        PolicyStatus.Quoted => "text-bg-secondary",
        PolicyStatus.Suspended => "text-bg-warning",
        PolicyStatus.Cancelled => "text-bg-danger",
        _ => "text-bg-light border"
    };

    public static string InvoiceBadge(InvoiceStatus status) => status switch
    {
        InvoiceStatus.Paid => "text-bg-success",
        InvoiceStatus.Issued => "text-bg-primary",
        InvoiceStatus.Overdue => "text-bg-danger",
        InvoiceStatus.CreditNote => "text-bg-info",
        InvoiceStatus.Cancelled => "text-bg-dark",
        _ => "text-bg-secondary"
    };
}

public class PageInfo
{
    public const int DefaultPageSize = 20;

    public PageInfo(int page, int total, int pageSize = DefaultPageSize)
    {
        PageSize = pageSize;
        Total = total;
        TotalPages = Math.Max(1, (int)Math.Ceiling(total / (double)pageSize));
        Page = Math.Clamp(page, 1, TotalPages);
    }

    public int Page { get; }

    public int PageSize { get; }

    public int Total { get; }

    public int TotalPages { get; }

    public int Skip => (Page - 1) * PageSize;
}
