using CQRS;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HomeController : ControllerBase
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IPublisher _publisher;

        public HomeController(ILogger<HomeController> logger, IPublisher publisher)
        {
            _logger = logger;
            _publisher = publisher;
        }

        [HttpGet(Name = "tesst")]
        public async Task<IActionResult> Get()
        {
            try
            {
                return Ok(await _publisher.Send(new Req()));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
