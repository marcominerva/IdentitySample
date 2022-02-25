using System.Security.Claims;
using IdentitySample.Authentication.Extensions;
using IdentitySample.Contracts;

namespace IdentitySample.Services;

public class HttpUserService : IUserService
{
    private readonly IHttpContextAccessor httpContextAccessor;

    public HttpUserService(IHttpContextAccessor httpContextAccessor)
    {
        this.httpContextAccessor = httpContextAccessor;
    }

    public Guid GetTenantId()
    {
        var tenantIdString = httpContextAccessor.HttpContext.User.GetClaimValue(ClaimTypes.GroupSid);
        if (Guid.TryParse(tenantIdString, out var tenantId))
        {
            return tenantId;
        }

        return Guid.Empty;
    }

    public string GetUserName() => httpContextAccessor.HttpContext.User.Identity.Name;
}
