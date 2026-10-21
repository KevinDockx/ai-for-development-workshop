using ClaimHub.Domain.Enums;

namespace ClaimHub.Domain.Entities;

public class ClaimDocument
{
    public int Id { get; set; }

    public int ClaimId { get; set; }

    public virtual Claim? Claim { get; set; }

    public DocumentType Type { get; set; } = DocumentType.Other;

    public string FileName { get; set; } = string.Empty;

    public string StoredPath { get; set; } = string.Empty;

    public string ContentType { get; set; } = "application/octet-stream";

    public long SizeBytes { get; set; }

    public DateTimeOffset UploadedAt { get; set; }

    public string UploadedBy { get; set; } = string.Empty;

    public string Extension => Path.GetExtension(FileName).ToLowerInvariant();

    public bool IsImage => ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
}
