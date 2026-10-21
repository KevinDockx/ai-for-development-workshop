using ClaimHub.Contracts.Auth;
using ClaimHub.Api.Security;
using ClaimHub.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClaimHub.Api.Controllers;

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly ClaimHubDbContext _db;
    private readonly IJwtTokenService _tokens;
    private readonly ILogger<AuthController> _logger;

    public AuthController(ClaimHubDbContext db, IJwtTokenService tokens, ILogger<AuthController> logger)
    {
        _db = db;
        _tokens = tokens;
        _logger = logger;
    }

    /// <summary>Exchanges an e-mail address and password for a bearer token.</summary>
    [HttpPost("token")]
    [ProducesResponseType<TokenResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateToken([FromBody] TokenRequest request, CancellationToken cancellationToken)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);
        if (user == null)
        {
            _logger.LogWarning("Token request for unknown user {Email}", request.Email);
            return NotFound(new ErrorResponse("user not found"));
        }

        if (!PasswordHasher.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Wrong password for user {Email}", request.Email);
            return Unauthorized(new ErrorResponse("wrong password"));
        }

        if (!user.IsActive)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ErrorResponse("account is disabled"));
        }

        user.RecordLogin(DateTimeOffset.UtcNow);
        await _db.SaveChangesAsync(cancellationToken);

        var (token, expiresAt) = _tokens.CreateToken(user);
        return Ok(new TokenResponse
        {
            Token = token,
            ExpiresAt = expiresAt,
            User = new UserInfo
            {
                Id = user.Id,
                Email = user.Email,
                DisplayName = user.DisplayName,
                Role = user.Role,
                PolicyHolderId = user.PolicyHolderId,
                BrokerId = user.BrokerId
            }
        });
    }
}
