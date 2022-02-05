using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace ConsumerService.Authentication;

public class ClaimsTransformer : IClaimsTransformation
{
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var identity = (ClaimsIdentity)principal.Identity;
        identity.AddClaim(new Claim("ApplicationId", "42"));

        var nameClaim = identity.FindFirst("customerusername");
        if (nameClaim != null)
        {
            identity.RemoveClaim(nameClaim);
            identity.AddClaim(new Claim(ClaimTypes.Name, nameClaim.Value));
        }

        return Task.FromResult(principal);
    }
}
