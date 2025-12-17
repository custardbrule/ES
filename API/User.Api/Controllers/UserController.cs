using CQRS;
using Infras.User.Services.Commands.User;
using Microsoft.AspNetCore.Mvc;

namespace User.Api.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly IPublisher _publisher;

        public UserController(ILogger<UserController> logger, IPublisher publisher)
        {
            _logger = logger;
            _publisher = publisher;
        }

        [HttpPost(Name = "Create User")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
            => Ok(await _publisher.Send<CreateUserRequest, Guid>(request));
    }
}
