namespace ClaimHub.Contracts.Common;

public record ApiErrorResponse(string? Error);

public record ApiErrorWithCodeResponse(string? Error, string? Code);

public record PolicyBalanceResponse(int PolicyId, decimal Outstanding);