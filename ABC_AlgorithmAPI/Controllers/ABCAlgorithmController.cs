using ABC_Algorithm;
using ABC_Algorithm.BenchMarks;
using Microsoft.AspNetCore.Mvc;

namespace ABC_AlgorithmAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ABCAlgorithmController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public ABCAlgorithmController( IConfiguration configuration)
        {
            _configuration = configuration; 
        }
        [HttpPost("Compute")]
        public IActionResult Compute([FromQuery] int FoodSources, [FromQuery] int Dimension, [FromQuery] int MaxCycles, [FromQuery] string BenchmarkFunction)
        {

            if (FoodSources <= 0 || Dimension <= 0 ||MaxCycles <= 0)
            {
                return BadRequest("All parameters must be positive integers.");
            }
            if (!Enum.TryParse<BenchmarkType>(BenchmarkFunction, true, out var selectedBenchmark))
            {
                return BadRequest("Invalid benchmark function specified.");
            }
            try
            {
                ArtificialBeeColony abc = new(FoodSources ,Dimension,100, MaxCycles,selectedBenchmark, _configuration);
                abc.Start();
                return Ok("ABC algorithm completed successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
            }
        }
        [HttpGet("GetBenchMark-Functions")]
        public IActionResult GetBenchMark()
        {
           return Ok(new
           {
               Benchmarks = new[]
                {
                    "Sphere",
                    "RotatedElliptic",
                    "RotatedBentCigar",
                    "RotatedDiscus",
                    "DifferentPowers",
                    "Rosenbrock",
                    "SchafferF7",
                    "Ackley",
                    "Weierstrass",
                    "Griewank",
                    "Rastrigin",
                    "RotatedKatsuura"
                }
           });

        }
    }
}
