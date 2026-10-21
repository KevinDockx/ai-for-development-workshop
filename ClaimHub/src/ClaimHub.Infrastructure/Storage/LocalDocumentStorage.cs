using Microsoft.Extensions.Options;

namespace ClaimHub.Infrastructure.Storage;

public class LocalDocumentStorage : IDocumentStorage
{
    public LocalDocumentStorage(IOptions<DocumentStorageOptions> options)
    {
        RootPath = DatabasePaths.ResolveFromSolutionRoot(options.Value.RootPath);
    }

    public string RootPath { get; }

    public async Task<StoredDocument> SaveAsync(string folder, string fileName, Stream content, CancellationToken cancellationToken = default)
    {
        string storedPath = Path.Combine(folder, fileName);
        long size = content.CanSeek ? content.Length : 0;

        try
        {
            string fullPath = Path.Combine(RootPath, storedPath);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            await using var target = File.Create(fullPath);
            await content.CopyToAsync(target, cancellationToken);
            size = target.Length;
        }
        catch
        {
        }

        return new StoredDocument(storedPath, size, DateTimeOffset.UtcNow);
    }

    public Stream OpenRead(string storedPath) =>
        File.OpenRead(Path.Combine(RootPath, storedPath));
}
