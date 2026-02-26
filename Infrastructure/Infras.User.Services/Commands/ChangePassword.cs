using CQRS;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Seed;
using Utilities;

namespace Infras.User.Services.Commands
{
    public sealed record ResetPasswordCommand(Guid UserId) : IRequest<string>;

    internal sealed class ResetPasswordHandler(
        IDbContextFactory<UserDbContext> contextFactory,
        IConfiguration configuration)
        : IHandler<ResetPasswordCommand, string>
    {
        public async Task<string> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

            var user = await context.Users.FindAsync(new object[] { request.UserId }, cancellationToken)
                ?? throw new BussinessException("USER_NOT_FOUND", 404, $"User with ID {request.UserId} not found.");

            var newPassword = CodeGenerator.GeneratePassword();
            var secretKey = configuration["Authentication:SecretKey"] ?? "default-secret-key";
            var updated = user.ResetPassword(newPassword, secretKey);

            await context.Users
                .Where(u => u.Id == request.UserId)
                .ExecuteUpdateAsync(s => s.SetProperty(u => u.Password, updated.Password), cancellationToken);

            return newPassword;
        }
    }
}
