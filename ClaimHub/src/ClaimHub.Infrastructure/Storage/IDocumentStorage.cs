namespace ClaimHub.Infrastructure.Storage;

public class DocumentStorageOptions
{
    public string RootPath { get; set; } = "data/documents";
}

public record StoredDocument(string StoredPath, long SizeBytes, DateTimeOffset StoredAt);

public interface IDocumentStorage
{
    string RootPath { get; }

    Task<StoredDocument> SaveAsync(string folder, string fileName, Stream content, CancellationToken cancellationToken = default);

    Stream OpenRead(string storedPath);
}
