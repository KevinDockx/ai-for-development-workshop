using System.Globalization;
using ClaimHub.Domain.Entities;
using ClaimHub.Infrastructure;
using ClaimHub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ClaimHub.Worker.Export;

public sealed record ExportFileResult(string Path, int PolicyHolders, int Policies, int Claims, long Bytes);

/// <summary>Reads the export data from the database and writes it as one JSON file.</summary>
public class NightlyExportWriter
{
    private static readonly JsonSerializerSettings SerializerSettings = new()
    {
        Formatting = Formatting.Indented,
        TypeNameHandling = TypeNameHandling.All,
        Converters = { new StringEnumConverter() }
    };

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly string _folder;
    private readonly string _filePrefix;

    public NightlyExportWriter(IServiceScopeFactory scopeFactory, string exportFolder, string filePrefix)
    {
        _scopeFactory = scopeFactory;
        _folder = DatabasePaths.ResolveFromSolutionRoot(exportFolder);
        _filePrefix = filePrefix;
    }

    public async Task<ExportFileResult> WriteAsync(DateTimeOffset generatedAt)
    {
        var export = await BuildAsync(generatedAt);

        Directory.CreateDirectory(_folder);
        string fileName = $"{_filePrefix}-{generatedAt.UtcDateTime:yyyyMMdd-HHmmss}.json";
        string path = Path.Combine(_folder, fileName);
        string temporaryPath = path + ".tmp";

        string json = JsonConvert.SerializeObject(export, SerializerSettings);
        await File.WriteAllTextAsync(temporaryPath, json);
        File.Move(temporaryPath, path, overwrite: true);

        return new ExportFileResult(
            path,
            export.PolicyHolders.Count,
            export.Policies.Count,
            export.Claims.Count,
            new FileInfo(path).Length);
    }

    private async Task<NightlyExport> BuildAsync(DateTimeOffset generatedAt)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ClaimHubDbContext>();

        var holders = await db.PolicyHolders.AsNoTracking()
            .Where(h => !h.IsDeleted)
            .OrderBy(h => h.Id)
            .ToListAsync();

        var policies = await db.Policies.AsNoTracking()
            .Where(p => !p.IsDeleted)
            .OrderBy(p => p.Id)
            .ToListAsync();

        var claims = await db.Claims.AsNoTracking()
            .Where(c => !c.IsDeleted)
            .OrderBy(c => c.Id)
            .ToListAsync();

        return new NightlyExport
        {
            GeneratedAt = generatedAt,
            PolicyHolders = holders.Select(ToExport).ToList(),
            Policies = policies.Select(ToExport).ToList(),
            Claims = claims.Select(ToExport).ToList()
        };
    }

    private static PolicyHolderExport ToExport(PolicyHolder holder) => new()
    {
        Id = holder.Id,
        FirstName = holder.FirstName,
        LastName = holder.LastName,
        Email = holder.Email,
        Phone = holder.Phone,
        DateOfBirth = Format(holder.DateOfBirth),
        NationalNumber = holder.NationalNumber,
        Iban = holder.Iban,
        Street = holder.Street,
        PostalCode = holder.PostalCode,
        City = holder.City,
        CustomerSince = Format(holder.CustomerSince),
        IsVip = holder.IsVip
    };

    private static PolicyExport ToExport(Policy policy) => new()
    {
        Id = policy.Id,
        PolicyNumber = policy.PolicyNumber,
        ProductType = policy.ProductType.ToString(),
        Status = policy.Status.ToString(),
        PolicyHolderId = policy.PolicyHolderId,
        StartDate = Format(policy.StartDate),
        EndDate = Format(policy.EndDate),
        AnnualPremium = policy.AnnualPremium,
        PaymentFrequency = policy.PaymentFrequency.ToString()
    };

    private static ClaimExport ToExport(Claim claim) => new()
    {
        Id = claim.Id,
        ClaimNumber = claim.ClaimNumber,
        PolicyId = claim.PolicyId,
        Status = claim.Status.ToString(),
        DateOfLoss = Format(claim.DateOfLoss),
        ReportedAt = claim.ReportedAt,
        Description = claim.Description,
        AssessedAmount = claim.AssessedAmount,
        ApprovedAmount = claim.ApprovedAmount,
        PaidAmount = claim.PaidAmount,
        AssignedHandlerId = claim.AssignedHandlerId,
        EscalatedAt = claim.EscalatedAt
    };

    private static string Format(DateOnly date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
}