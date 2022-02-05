using System.Security.Claims;
using System.Security.Principal;

namespace IdentitySample.Authentication.Extensions;

public static class ClaimsExtensions
{
    public static Guid GetId(this IPrincipal user)
    {
        var value = GetClaimValue(user, ClaimTypes.NameIdentifier);
        return Guid.Parse(value);
    }

    public static string GetFirstName(this IPrincipal user)
        => GetClaimValue(user, ClaimTypes.GivenName);

    public static string GetLastName(this IPrincipal user)
        => GetClaimValue(user, ClaimTypes.Surname);

    public static string GetEmail(this IPrincipal user)
        => GetClaimValue(user, ClaimTypes.Email);

    public static int GetApplicationId(this IPrincipal user)
    {
        var value = GetClaimValue(user, CustomClaimTypes.ApplicationId);
        return int.Parse(value);
    }

    public static IEnumerable<string> GetRoles(this IPrincipal user)
    {
        var values = ((ClaimsPrincipal)user).FindAll(ClaimTypes.Role).Select(c => c.Value);
        return values;
    }

    public static string GetClaimValue(this IPrincipal user, string claimType)
    {
        var value = ((ClaimsPrincipal)user).FindFirst(claimType)?.Value;
        return value;
    }
}
