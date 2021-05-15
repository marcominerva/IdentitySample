using Microsoft.AspNetCore.Mvc;

namespace ConsumerService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MeController : ControllerBase
    {
        public MeController()
        {
        }

        [HttpGet]
        public IActionResult Get()
        {
            return NoContent();
        }
    }
}
