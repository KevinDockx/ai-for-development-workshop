using System.Security.Claims;
using ClaimHub.Domain.Enums;

namespace ClaimHub.Api.Security;

public static class ClaimsPrincipalExtensions
{
    public const string RoleClaim = "role";
    public const string PolicyHolderIdClaim = "policyHolderId";
    public const string BrokerIdClaim = "brokerId";

    public static int GetUserId(this ClaimsPrincipal user) =>
        int.TryParse(user.FindFirstValue("sub"), out int id) ? id : 0;

    public static string GetEmail(this ClaimsPrincipal user) =>
        user.FindFirstValue("email") ?? "unknown";

    public static UserRole? GetRole(this ClaimsPrincipal user) =>
        Enum.TryParse<UserRole>(user.FindFirstValue(RoleClaim), out var role) ? role : null;

    public static int? GetPolicyHolderId(this ClaimsPrincipal user) =>
        int.TryParse(user.FindFirstValue(PolicyHolderIdClaim), out int id) ? id : null;

    public static int? GetBrokerId(this ClaimsPrincipal user) =>
        int.TryParse(user.FindFirstValue(BrokerIdClaim), out int id) ? id : null;

    public static bool IsStaff(this ClaimsPrincipal user) =>
        user.GetRole() is not (UserRole.PolicyHolder or UserRole.Broker or null);
}
