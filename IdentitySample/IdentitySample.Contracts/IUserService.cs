namespace IdentitySample.Contracts;

public interface IUserService
{
    string GetUserName();

    Guid GetTenantId();
}
