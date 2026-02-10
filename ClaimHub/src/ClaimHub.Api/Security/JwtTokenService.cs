using System.Security.Claims;
using Claim = System.Security.Claims.Claim;
using System.Text;
using ClaimHub.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace ClaimHub.Api.Security;

public interface IJwtTokenService
{
    (string Token, DateTimeOffset ExpiresAt) CreateToken(ApplicationUser user);
}

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings _settings;

    public JwtTokenService(IOptions<JwtSettings> options)
    {
        _settings = options.Value;
    }

    public static SymmetricSecurityKey BuildKey(string signingKey) =>
        new(Encoding.UTF8.GetBytes(signingKey));

    public (string Token, DateTimeOffset ExpiresAt) CreateToken(ApplicationUser user)
    {
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(_settings.ExpiryMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimsPrincipalExtensions.RoleClaim, user.Role.ToString()),
            new("name", user.DisplayName)
        };

        if (user.PolicyHolderId.HasValue)
        {
            claims.Add(new Claim(ClaimsPrincipalExtensions.PolicyHolderIdClaim, user.PolicyHolderId.Value.ToString()));
        }

        if (user.BrokerId.HasValue)
        {
            claims.Add(new Claim(ClaimsPrincipalExtensions.BrokerIdClaim, user.BrokerId.Value.ToString()));
        }

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = _settings.Issuer,
            Audience = _settings.Audience,
            Expires = expiresAt.UtcDateTime,
            SigningCredentials = new SigningCredentials(BuildKey(_settings.SigningKey), SecurityAlgorithms.HmacSha256)
        };

        string token = new JsonWebTokenHandler().CreateToken(descriptor);
        return (token, expiresAt);
    }
}
