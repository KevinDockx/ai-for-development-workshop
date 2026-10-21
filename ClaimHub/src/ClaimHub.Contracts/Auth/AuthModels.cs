using System.ComponentModel.DataAnnotations;
using ClaimHub.Domain.Enums;

namespace ClaimHub.Contracts.Auth;

public class TokenRequest
{
    [Required]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class UserInfo
{
    public int Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    public int? PolicyHolderId { get; set; }

    public int? BrokerId { get; set; }
}

public class TokenResponse
{
    public string Token { get; set; } = string.Empty;

    public string TokenType { get; set; } = "Bearer";

    public DateTimeOffset ExpiresAt { get; set; }

    public UserInfo User { get; set; } = new();
}

public class ErrorResponse
{
    public ErrorResponse(string message)
    {
        Message = message;
    }

    public string Message { get; }
}
