using System.Text;

namespace ClaimHub.Infrastructure.Reporting;

public static class PolicyHolderCsvFormatter
{
    public const string Header = "Id,FirstName,LastName,Email,Phone,DateOfBirth,NationalNumber,Iban,Street,PostalCode,City,CustomerSince";

    public static string Format(IEnumerable<PolicyHolderExportRow> rows)
    {
        ArgumentNullException.ThrowIfNull(rows);

        var csv = new StringBuilder();
        csv.AppendLine(Header);
        foreach (var row in rows)
        {
            csv.AppendLine(Line(row));
        }

        return csv.ToString();
    }

    public static string Line(PolicyHolderExportRow h) =>
        string.Join(',',
            h.Id, h.FirstName, h.LastName, h.Email, h.Phone, h.DateOfBirth.ToString("yyyy-MM-dd"),
            h.NationalNumber, h.Iban, $"\"{h.Street}\"", h.PostalCode, h.City, h.CustomerSince.ToString("yyyy-MM-dd"));
}