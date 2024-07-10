using IdentitySample.BusinessLayer.Services;
using Microsoft.AspNetCore.Mvc;

namespace IdentitySample.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthenticatedController(IAuthenticatedService authenticatedService) : ControllerBase
{

    /// <summary>
    /// Open the AuthenticatedService.RunAsync method to see the correct way to retrieve the user name from a Business Layer Service,
    /// without explicitly use the HttpContext (refer also to the IUserService interface).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Run()
    {
        await authenticatedService.RunAsync();
        return NoContent();
    }
}
