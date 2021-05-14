using IdentitySample.Authentication.Extensions;
using IdentitySample.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace IdentitySample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MeController : ControllerBase
    {
        //[Authorize]
        [HttpGet]
        public IActionResult GetMe()
        {
            var user = new User
            {
                FirstName = User.GetFirstName(),
                LastName = User.GetLastName(),
                Email = User.GetEmail()
            };

            return Ok(user);
        }
    }
}
