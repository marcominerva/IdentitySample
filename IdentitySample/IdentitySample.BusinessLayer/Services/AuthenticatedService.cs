using System.Threading.Tasks;

namespace IdentitySample.BusinessLayer.Services
{
    public class AuthenticatedService : IAuthenticatedService
    {
        private readonly IUserService userService;

        public AuthenticatedService(IUserService userService)
        {
            this.userService = userService;
        }

        public Task RunAsync()
        {
            var userName = userService.GetUserName();
            return Task.CompletedTask;
        }
    }
}
