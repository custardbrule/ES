using CQRS;
using Infras.Services.Commands.Diary;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class DiaryController : ControllerBase
    {
        private readonly ILogger<DiaryController> _logger;
        private readonly IPublisher _publisher;

        public DiaryController(ILogger<DiaryController> logger, IPublisher publisher)
        {
            _logger = logger;
            _publisher = publisher;
        }

        [HttpPost(Name = "Add Diary")]
        public async Task<IActionResult> AddDiary([FromBody] CreateDiaryRequest request) => Ok(await _publisher.Send<CreateDiaryRequest, long>(request));

        [HttpPost(Name = "Add Day")]
        public async Task<IActionResult> AddDay([FromBody] AddDayRequest request) => Ok(await _publisher.Send<AddDayRequest, long>(request));

        [HttpPost(Name = "Add Section")]
        public async Task<IActionResult> AddDay([FromBody] AddDiarySectionRequest request) => Ok(await _publisher.Send<AddDiarySectionRequest, long>(request));
    }
}
