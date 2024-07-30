namespace ClaimHub.Contracts.Documents;

public record DocumentUploadResponse(int Id, string FileName, string StoredPath, long SizeBytes);