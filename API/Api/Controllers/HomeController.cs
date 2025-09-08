using CQRS;
using Data;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HomeController : ControllerBase
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IPublisher _publisher;
        private readonly IQuartzJobManager _quartzJobManager;

        public HomeController(ILogger<HomeController> logger, IPublisher publisher, IQuartzJobManager quartzJobManager)
        {
            _logger = logger;
            _publisher = publisher;
            _quartzJobManager = quartzJobManager;
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

        [HttpGet("test2",Name = "tesstessst")]
        public async Task<IActionResult> GetTest2()
        {
            try
            {
                await _quartzJobManager.Fire<TJob>(CancellationToken.None);
                return Ok(await _publisher.Send(new Req()));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
