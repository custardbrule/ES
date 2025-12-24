using CQRS;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Infras.User.Services.Commands
{
    public sealed record AuthenticateUserCommand(
        string Account,
        string Password
    ) : IRequest<Domain.User.UserRoot.User?>;

    internal sealed class AuthenticateUserHandler : IHandler<AuthenticateUserCommand, Domain.User.UserRoot.User?>
    {
        private readonly UserDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthenticateUserHandler(UserDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<Domain.User.UserRoot.User?> Handle(AuthenticateUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .Include(u => u.Scopes)
                .FirstOrDefaultAsync(u => u.Account == request.Account, cancellationToken);

            var secretKey = _configuration["Authentication:SecretKey"] ?? "default-secret-key";

            if (user == null || !user.ValidatePassword(request.Password, secretKey))
            {
                return null;
            }

            return user;
        }
    }
}
