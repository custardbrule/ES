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
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IPublisher _publisher;

        public UserController(IPublisher publisher)
        {
            _publisher = publisher;
        }

        // GET: api/User
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] GetAllUsersQuery query)
        {
            var users = await _publisher.Send<GetAllUsersQuery, PagedList<UserDto>>(query);
            return Ok(users);
        }

        // GET: api/User/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var user = await _publisher.Send<GetUserDetailsQuery, UserDetailsDto>(new GetUserDetailsQuery(id));
            return Ok(user);
        }

        // POST: api/User
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] RegisterUserCommand request)
        {
            var result = await _publisher.Send<RegisterUserCommand, RegisterUserResult>(request);
            return CreatedAtAction(nameof(GetById), new { id = result.UserId }, result);
        }

        // DELETE: api/User/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _publisher.Send<DeleteUserCommand, Unit>(new DeleteUserCommand(id));
            return NoContent();
        }

        // POST: api/User/{id}/password
        [HttpPost("{id:guid}/password")]
        public async Task<IActionResult> ResetPassword(Guid id)
        {
            var password = await _publisher.Send<ResetPasswordCommand, string>(new ResetPasswordCommand(id));
            return Ok(new { password });
        }

        // POST: api/User/{id}/roles/{roleId}
        [HttpPost("{id:guid}/roles/{roleId:guid}")]
        public async Task<IActionResult> AssignRole(Guid id, Guid roleId)
        {
            await _publisher.Send<AssignRoleToUserCommand, Unit>(new AssignRoleToUserCommand(id, roleId));
            return NoContent();
        }

        // DELETE: api/User/{id}/roles/{roleId}
        [HttpDelete("{id:guid}/roles/{roleId:guid}")]
        public async Task<IActionResult> RemoveRole(Guid id, Guid roleId)
        {
            await _publisher.Send<RemoveRoleFromUserCommand, Unit>(new RemoveRoleFromUserCommand(id, roleId));
            return NoContent();
        }
    }

}
