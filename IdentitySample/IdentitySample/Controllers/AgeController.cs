using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentitySample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AgeController : ControllerBase
    {
        [HttpGet("18")]
        [Authorize(Policy = "AtLeast18")]
        public IActionResult AtLeast18()
        {
            return NoContent();
        }

        [HttpGet("21")]
        [Authorize(Policy = "AtLeast21")]
        public IActionResult AtLeast21()
        {
            return NoContent();
        }
    }
}
