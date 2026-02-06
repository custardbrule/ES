using CQRS;
using Infras.User.Services.Commands;
using Infras.User.Services.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Utilities;

namespace User.Api.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class ApplicationController : ControllerBase
    {
        private readonly IPublisher _publisher;

        public ApplicationController(IPublisher publisher)
        {
            _publisher = publisher;
        }

        // GET: api/Application
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] GetAllApplicationsQuery query)
        {
            var applications = await _publisher.Send<GetAllApplicationsQuery, PagedList<ApplicationDto>>(query);

            return Ok(applications);
        }

        // GET: api/Application/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var application = await _publisher.Send<GetApplicationByIdQuery, ApplicationDetailsDto>(
                new GetApplicationByIdQuery(id));

            return Ok(application);
        }

        // POST: api/Application
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateApplicationCommand request)
        {
            var id = await _publisher.Send<CreateApplicationCommand, string>(request);

            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }

        // PUT: api/Application/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateApplicationCommand request)
        {
            await _publisher.Send<UpdateApplicationCommand, Unit>(request);

            return NoContent();
        }

        // DELETE: api/Application/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            await _publisher.Send<DeleteApplicationCommand, Unit>(new DeleteApplicationCommand(id));

            return NoContent();
        }
    }
}
