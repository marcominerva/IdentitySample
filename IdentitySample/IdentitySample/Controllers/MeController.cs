using IdentitySample.Authentication;
using IdentitySample.Authentication.Extensions;
using IdentitySample.Authentication.Filters;
using Microsoft.AspNetCore.Mvc;

namespace IdentitySample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MeController : ControllerBase
    {
        [HttpGet]
        [RoleAuthorize(RoleNames.Administrator, RoleNames.PowerUser)]
        public IActionResult GetMe()
        {
            var applicationId = User.GetApplicationId();
            var userId = User.GetId();
            var isAdmninistrator = User.IsInRole(RoleNames.Administrator);
            var roles = User.GetRoles();

            return NoContent();
        }
    }
}
