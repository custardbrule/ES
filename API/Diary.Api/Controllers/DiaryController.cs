using CQRS;
using Infras.Diary.Services.Commands.Diary;
using Infras.Diary.Services.Queries.Diary;
using Infras.Diary.Services.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Utilities;

namespace Diary.Api.Controllers
{
    [Authorize]
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

        [HttpPost(Name = "Add Section")]
        public async Task<IActionResult> AddSection([FromBody] AddDiarySectionRequest request) => Ok(await _publisher.Send<AddDiarySectionRequest, long>(request));

        [HttpPost(Name = "Add Section To Day")]
        public async Task<IActionResult> AddSectionToDay([FromBody] AddSectionToDayRequest request) => Ok(await _publisher.Send<AddSectionToDayRequest, long>(request));

        [HttpDelete(Name = "Remove Section")]
        public async Task<IActionResult> RemoveSection([FromBody] RemoveDiarySectionRequest request) => Ok(await _publisher.Send<RemoveDiarySectionRequest, long>(request));

        [HttpPatch("{id}", Name = "Change Diary Visibility")]
        public async Task<IActionResult> ChangeDiaryVisibility(Guid id, [FromBody] ChangeDiaryVisibilityBody body) => Ok(await _publisher.Send<ChangeDiaryVisibilityRequest, long>(new(id, body.Visibility)));

        [HttpPatch("{id}", Name = "Change Diary Info")]
        public async Task<IActionResult> ChangeDiaryInfo(Guid id, [FromBody] ChangeDiaryInfoBody body) => Ok(await _publisher.Send<ChangeDiaryInfoRequest, long>(new(id, body.Name, body.Description)));

        [HttpPatch("{id}", Name = "Pin Section")]
        public async Task<IActionResult> PinSection(Guid id, [FromBody] PinSectionBody body) => Ok(await _publisher.Send<PinSectionRequest, long>(new(id, body.TimeZoneId, body.SectionId, body.IsPinned)));
    }
}
