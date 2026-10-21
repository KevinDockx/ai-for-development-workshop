using ClaimHub.Api.Security;
using ClaimHub.Contracts.Common;
using ClaimHub.Contracts.Documents;
using ClaimHub.Domain.Entities;
using ClaimHub.Domain.Enums;
using ClaimHub.Infrastructure.Data;
using ClaimHub.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;

namespace ClaimHub.Api.Endpoints;

public static class DocumentEndpoints
{
    public static IEndpointRouteBuilder MapDocumentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/documents")
            .WithTags("Documents")
            .RequireAuthorization();

        group.MapPost("/claims/{claimId:int}", UploadAsync).DisableAntiforgery();
        group.MapGet("/{documentId:int}/download", DownloadAsync);

        return app;
    }

    private static async Task<IResult> UploadAsync(
        int claimId,
        IFormFile file,
        DocumentType? type,
        ClaimHubDbContext db,
        IDocumentStorage storage,
        HttpContext http,
        CancellationToken cancellationToken)
    {
        var claim = await db.Claims.FirstOrDefaultAsync(c => c.Id == claimId && !c.IsDeleted, cancellationToken);
        if (claim == null)
        {
            return Results.NotFound(new ApiErrorResponse($"Claim {claimId} was not found."));
        }

        await using var stream = file.OpenReadStream();
        var stored = await storage.SaveAsync(claim.ClaimNumber, file.FileName, stream, cancellationToken);

        var document = new ClaimDocument
        {
            ClaimId = claim.Id,
            Type = type ?? DocumentType.Other,
            FileName = file.FileName,
            StoredPath = stored.StoredPath,
            ContentType = file.ContentType,
            SizeBytes = stored.SizeBytes,
            UploadedAt = stored.StoredAt,
            UploadedBy = http.User.GetEmail()
        };

        db.ClaimDocuments.Add(document);
        await db.SaveChangesAsync(cancellationToken);

        return Results.Created($"/api/documents/{document.Id}/download", new DocumentUploadResponse(
            document.Id,
            document.FileName,
            document.StoredPath,
            document.SizeBytes));
    }

    private static async Task<IResult> DownloadAsync(
        int documentId,
        string? fileName,
        ClaimHubDbContext db,
        IDocumentStorage storage,
        CancellationToken cancellationToken)
    {
        var document = await db.ClaimDocuments.AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == documentId, cancellationToken);
        if (document == null)
        {
            return Results.NotFound(new ApiErrorResponse($"Document {documentId} was not found."));
        }

        var requested = string.IsNullOrWhiteSpace(fileName) ? document.StoredPath : fileName;
        var fullPath = Path.Combine(storage.RootPath, requested);
        if (!File.Exists(fullPath))
        {
            return Results.NotFound(new ApiErrorResponse("The file is not available."));
        }

        var bytes = await File.ReadAllBytesAsync(fullPath, cancellationToken);
        return Results.File(bytes, document.ContentType, Path.GetFileName(fullPath));
    }
}
