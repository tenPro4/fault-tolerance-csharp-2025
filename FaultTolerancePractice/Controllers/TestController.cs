using FaultTolerancePractice.HttpClients;
using Microsoft.AspNetCore.Mvc;

namespace FaultTolerancePractice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class TestController : ControllerBase
    {
        private readonly WeatherServiceClient _weatherServiceClient;

        public TestController(WeatherServiceClient weatherServiceClient)
        {
            _weatherServiceClient = weatherServiceClient;
        }

        [HttpGet]
        public async Task<IActionResult> Index() {

            var result = await _weatherServiceClient.GetWeatherForecast();
            return Ok(result);
        }
    }
}
