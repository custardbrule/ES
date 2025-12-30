using CQRS;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Infras.User.Services.Commands
{
    public sealed record AuthenticateUserCommand(
        string Account,
        string Password
    ) : IRequest<Domain.User.UserRoot.User?>;

    internal sealed class AuthenticateUserHandler(
        IDbContextFactory<UserDbContext> contextFactory,
        IConfiguration configuration) : IHandler<AuthenticateUserCommand, Domain.User.UserRoot.User?>
    {
        public async Task<Domain.User.UserRoot.User?> Handle(AuthenticateUserCommand request, CancellationToken cancellationToken)
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

            var user = await context.Users
                .Include(u => u.UserRoles)
                .Include(u => u.Scopes)
                .FirstOrDefaultAsync(u => u.Account == request.Account, cancellationToken);

            var secretKey = configuration["Authentication:SecretKey"] ?? "default-secret-key";

            if (user == null || !user.ValidatePassword(request.Password, secretKey))
            {
                return null;
            }

            return user;
        }
    }
}
