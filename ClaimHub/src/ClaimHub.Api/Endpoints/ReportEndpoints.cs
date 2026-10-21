using System.Text;
using ClaimHub.Contracts.Reports;
using ClaimHub.Infrastructure.Reporting;

namespace ClaimHub.Api.Endpoints;

public static class ReportEndpoints
{
    public static IEndpointRouteBuilder MapReportEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/reports")
            .WithTags("Reports")
            .RequireAuthorization();

        group.MapGet("/export", ExportAsync);

        return app;
    }

    private static async Task<IResult> ExportAsync(string? format, PolicyHolderExportQuery query, CancellationToken cancellationToken)
    {
        var holders = await query.GetRowsAsync(cancellationToken);

        if (!string.Equals(format, "csv", StringComparison.OrdinalIgnoreCase))
        {
            return Results.Ok(holders.Select(ToItem).ToList());
        }

        return Results.Text(PolicyHolderCsvFormatter.Format(holders), "text/csv", Encoding.UTF8);
    }

    private static PolicyHolderExportItem ToItem(PolicyHolderExportRow row) => new(
        row.Id,
        row.FirstName,
        row.LastName,
        row.Email,
        row.Phone,
        row.DateOfBirth,
        row.NationalNumber,
        row.Iban,
        row.Street,
        row.PostalCode,
        row.City,
        row.CustomerSince);
}
