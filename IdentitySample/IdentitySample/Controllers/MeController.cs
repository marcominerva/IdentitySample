using IdentitySample.Authentication.Extensions;
using IdentitySample.Contracts;
using IdentitySample.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace IdentitySample.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MeController : ControllerBase
{
    //[Authorize]
    [HttpGet]
    public IActionResult GetMe([FromServices] IUserService userService)
    {
        var user = new User
        {
            Id = User.GetId(),
            FirstName = User.GetFirstName(),
            LastName = User.GetLastName(),
            Email = User.GetEmail(),
            TenantId = userService.GetTenantId()
        };

        return Ok(user);
    }
}
