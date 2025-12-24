using CQRS;
using Domain.User.UserRoot;

namespace Infras.User.Services.Commands
{
    public sealed record LogLoginHistoryCommand(
        Guid UserId,
        string IpAddress,
        string UserAgent
    ) : IRequest;

    internal sealed class LogLoginHistoryHandler : IHandler<LogLoginHistoryCommand>
    {
        private readonly UserDbContext _context;

        public LogLoginHistoryHandler(UserDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(LogLoginHistoryCommand request, CancellationToken cancellationToken)
        {
            var loginHistory = new LoginHistory(
                Guid.NewGuid(),
                request.UserId,
                DateTimeOffset.UtcNow,
                request.IpAddress,
                request.UserAgent
            );

            _context.LoginHistories.Add(loginHistory);
            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
