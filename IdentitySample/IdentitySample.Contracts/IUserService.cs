using System.Security.Claims;

namespace IdentitySample.Contracts;

public interface IUserService
{
    string GetUserName();

    Guid GetTenantId();

    public ClaimsIdentity GetIdentity();
}
