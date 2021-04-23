using System.Threading.Tasks;
using IdentitySample.BusinessLayer.Services;
using IdentitySample.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentitySample.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IIdentityService identityService;

        public AuthController(IIdentityService identityService)
        {
            this.identityService = identityService;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var response = await identityService.LoginAsync(request);
            if (response != null)
            {
                return Ok(response);
            }

            return BadRequest();
        }
    }
}
