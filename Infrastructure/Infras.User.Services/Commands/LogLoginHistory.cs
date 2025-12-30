using CQRS;
using Domain.User.UserRoot;
using Microsoft.EntityFrameworkCore;

namespace Infras.User.Services.Commands
{
    public sealed record LogLoginHistoryCommand(
        Guid UserId,
        string IpAddress,
        string UserAgent
    ) : IRequest;

    internal sealed class LogLoginHistoryHandler(IDbContextFactory<UserDbContext> contextFactory)
        : IHandler<LogLoginHistoryCommand>
    {
        public async Task<Unit> Handle(LogLoginHistoryCommand request, CancellationToken cancellationToken)
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

            var loginHistory = new LoginHistory(
                Guid.NewGuid(),
                request.UserId,
                DateTimeOffset.UtcNow,
                request.IpAddress,
                request.UserAgent
            );

            context.LoginHistories.Add(loginHistory);
            await context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
