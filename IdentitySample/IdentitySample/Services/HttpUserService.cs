using IdentitySample.BusinessLayer.Services;
using Microsoft.AspNetCore.Http;

namespace IdentitySample.Services
{
    public class HttpUserService : IUserService
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public HttpUserService(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public string GetUserName()
        {
            return httpContextAccessor.HttpContext.User.Identity.Name;
        }
    }
}
