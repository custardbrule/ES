using CQRS;
using Infras.User.Services.Commands;
using Infras.User.Services.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace User.Api.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ScopeController : ControllerBase
    {
        private readonly IPublisher _publisher;

        public ScopeController(IPublisher publisher)
        {
            _publisher = publisher;
        }

        // GET: api/Scope
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var scopes = await _publisher.Send<GetAllScopesQuery, List<ScopeDto>>(
                new GetAllScopesQuery());

            return Ok(scopes);
        }

        // GET: api/Scope/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var scope = await _publisher.Send<GetScopeByIdQuery, ScopeDetailsDto>(
                new GetScopeByIdQuery(id));

            return Ok(scope);
        }

        // POST: api/Scope
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateScopeCommand request)
        {
            var id = await _publisher.Send<CreateScopeCommand, string>(request);

            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }

        // PUT: api/Scope/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateScopeCommand request)
        {
            await _publisher.Send<UpdateScopeCommand, Unit>(request);

            return NoContent();
        }

        // DELETE: api/Scope/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            await _publisher.Send<DeleteScopeCommand, Unit>(new DeleteScopeCommand(id));

            return NoContent();
        }
    }
}
