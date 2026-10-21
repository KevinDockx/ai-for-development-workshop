namespace ClaimHub.Api.Security;

public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string SigningKey { get; set; } = string.Empty;

    public string Issuer { get; set; } = "claimhub-api";

    public string Audience { get; set; } = "claimhub-clients";

    // Partner integrations still cache tokens for weeks, so the default lifetime is 30 days.
    public int ExpiryMinutes { get; set; } = 43200;
}
