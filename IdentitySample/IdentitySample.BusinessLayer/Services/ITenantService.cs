using IdentitySample.BusinessLayer.Models;

namespace IdentitySample.BusinessLayer.Services;

public interface ITenantService
{
    Tenant GetTenant();
}