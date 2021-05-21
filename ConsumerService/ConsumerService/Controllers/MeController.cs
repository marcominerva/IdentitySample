using Microsoft.AspNetCore.Mvc;

namespace ConsumerService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MeController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            var user = User;
            return NoContent();
        }
    }
}
