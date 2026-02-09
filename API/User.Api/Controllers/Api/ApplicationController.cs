using CQRS;
using Infras.User.Services.Commands;
using Infras.User.Services.Dtos;
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
            var result = await _publisher.Send<CreateApplicationCommand, CreateApplicationResult>(request);

            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        // PUT: api/Application/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateApplicationBody body)
        {
            var result = await _publisher.Send<UpdateApplicationCommand, UpdateApplicationResult>(new UpdateApplicationCommand(id, body));

            return Ok(result);
        }

        // DELETE: api/Application/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            await _publisher.Send<DeleteApplicationCommand, Unit>(new DeleteApplicationCommand(id));

            return NoContent();
        }

        // GET: api/Application/{applicationId}/roles
        [HttpGet("{applicationId}/roles")]
        public async Task<IActionResult> GetRoles(string applicationId)
        {
            var roles = await _publisher.Send<GetApplicationRolesQuery, List<ApplicationRoleDto>>(
                new GetApplicationRolesQuery(applicationId));

            return Ok(roles);
        }

        // POST: api/Application/{applicationId}/roles
        [HttpPost("{applicationId}/roles")]
        public async Task<IActionResult> CreateRole(string applicationId, [FromBody] CreateApplicationRoleBody body)
        {
            var id = await _publisher.Send<CreateApplicationRoleCommand, Guid>(
                new CreateApplicationRoleCommand(applicationId, body));

            return Created($"api/Application/{applicationId}/roles/{id}", new { id });
        }

        // PUT: api/Application/{applicationId}/roles/{roleId}
        [HttpPut("{applicationId}/roles/{roleId}")]
        public async Task<IActionResult> UpdateRole(string applicationId, Guid roleId, [FromBody] UpdateApplicationRoleBody body)
        {
            await _publisher.Send<UpdateApplicationRoleCommand, Unit>(
                new UpdateApplicationRoleCommand(applicationId, roleId, body));

            return NoContent();
        }

        // DELETE: api/Application/{applicationId}/roles/{roleId}
        [HttpDelete("{applicationId}/roles/{roleId}")]
        public async Task<IActionResult> DeleteRole(string applicationId, Guid roleId)
        {
            await _publisher.Send<DeleteApplicationRoleCommand, Unit>(
                new DeleteApplicationRoleCommand(applicationId, roleId));

            return NoContent();
        }

        // GET: api/Application/{applicationId}/scopes
        [HttpGet("{applicationId}/scopes")]
        public async Task<IActionResult> GetScopes(string applicationId)
        {
            var scopes = await _publisher.Send<GetApplicationScopesQuery, List<ApplicationScopeDto>>(
                new GetApplicationScopesQuery(applicationId));

            return Ok(scopes);
        }

        // PUT: api/Application/{applicationId}/scopes
        [HttpPut("{applicationId}/scopes")]
        public async Task<IActionResult> ReplaceScopes(string applicationId, [FromBody] List<string> names)
        {
            await _publisher.Send<ReplaceApplicationScopesCommand, Unit>(
                new ReplaceApplicationScopesCommand(applicationId, names));

            return NoContent();
        }
    }
}
