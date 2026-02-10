using CQRS;
using Data;
using Microsoft.AspNetCore.Mvc;

namespace Diary.Api.Controllers
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
                return Ok(await _publisher.Send<Req2, int>(new Req2()));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
