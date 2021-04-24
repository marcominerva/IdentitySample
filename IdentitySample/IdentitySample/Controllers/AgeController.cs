using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentitySample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AgeController : ControllerBase
    {
        [HttpGet]
        [Authorize(Policy = "AtLeast18")]
        public IActionResult AtLeast18()
        {
            return NoContent();
        }

        [HttpGet]
        [Authorize(Policy = "AtLeast21")]
        public IActionResult AtLeast21()
        {
            return NoContent();
        }
    }
}
