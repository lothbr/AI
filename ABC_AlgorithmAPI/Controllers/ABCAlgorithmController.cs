using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ABC_AlgorithmAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ABCAlgorithmController : ControllerBase
    {
        [HttpPost("Compute")]
        public IActionResult Compute([FromQuery] int FoodSources, [FromQuery] int Dimension, [FromQuery] int Limit, [FromQuery] int MaxCycles)
        {
            return Ok();
        }
    }
}
