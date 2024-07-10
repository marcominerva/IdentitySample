using IdentitySample.Contracts;

namespace IdentitySample.BusinessLayer.Services;

public class AuthenticatedService(IUserService userService) : IAuthenticatedService
{
    public Task RunAsync()
    {
        var userName = userService.GetUserName();
        return Task.CompletedTask;
    }
}
