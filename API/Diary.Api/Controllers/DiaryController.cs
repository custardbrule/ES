using CQRS;
using Infras.Diary.Services.Commands.Diary;
using Infras.Diary.Services.Queries.Diary;
using Infras.Diary.Services.Queries;
using Microsoft.AspNetCore.Mvc;
using Utilities;

namespace Diary.Api.Controllers
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

        [HttpGet(Name = "Get Diaries")]
        public async Task<IActionResult> GetDiaries([FromQuery] GetDiariesRequest request) => Ok(await _publisher.Send<GetDiariesRequest, EPagedList<DiaryViewModel>>(request));

        [HttpPost(Name = "Add Diary")]
        public async Task<IActionResult> AddDiary([FromBody] CreateDiaryRequest request) => Ok(await _publisher.Send<CreateDiaryRequest, long>(request));

        [HttpPost(Name = "Add Day")]
        public async Task<IActionResult> AddDay([FromBody] AddDayRequest request) => Ok(await _publisher.Send<AddDayRequest, long>(request));

        [HttpPost(Name = "Add Section")]
        public async Task<IActionResult> AddSection([FromBody] AddDiarySectionRequest request) => Ok(await _publisher.Send<AddDiarySectionRequest, long>(request));

        [HttpPatch(Name = "Change Diary Visibility")]
        public async Task<IActionResult> ChangeDiaryVisibility([FromBody] ChangeDiaryVisibilityRequest request) => Ok(await _publisher.Send<ChangeDiaryVisibilityRequest, long>(request));

        [HttpPatch(Name = "Change Diary Info")]
        public async Task<IActionResult> ChangeDiaryInfo([FromBody] ChangeDiaryInfoRequest request) => Ok(await _publisher.Send<ChangeDiaryInfoRequest, long>(request));

        [HttpPatch(Name = "Pin Section")]
        public async Task<IActionResult> PinSection([FromBody] PinSectionRequest request) => Ok(await _publisher.Send<PinSectionRequest, long>(request));
    }
}
