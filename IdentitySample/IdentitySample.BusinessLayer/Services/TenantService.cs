using IdentitySample.Authentication;
using IdentitySample.BusinessLayer.Models;
using IdentitySample.Contracts;
using Microsoft.Extensions.Caching.Memory;

namespace IdentitySample.BusinessLayer.Services;

public class TenantService : ITenantService
{
    private readonly IUserService userService;
    private readonly AuthenticationDbContext authenticationDbContext;
    private readonly IMemoryCache cache;

    public TenantService(IUserService userService, AuthenticationDbContext authenticationDbContext,
        IMemoryCache cache)
    {
        this.userService = userService;
        this.authenticationDbContext = authenticationDbContext;
        this.cache = cache;
    }

    public Tenant GetTenant()
    {
        var tenants = cache.GetOrCreate("tenants", entry =>
        {
            var tenants = authenticationDbContext.Tenants.ToList()
                .ToDictionary(k => k.Id, v => new Tenant(v.Id, v.ConnectionString));

            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
            return tenants;
        });

        var tenantId = userService.GetTenantId();
        if (tenants.TryGetValue(tenantId, out var tenant))
        {
            return tenant;
        }

        return null;
    }

    public void ClearCache()
        => cache.Remove("tenants");
}
