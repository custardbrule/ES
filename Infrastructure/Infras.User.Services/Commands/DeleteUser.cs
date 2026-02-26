using CQRS;
using Microsoft.EntityFrameworkCore;
using Seed;
using Utilities;

namespace Infras.User.Services.Commands
{
    public sealed record DeleteUserCommand(Guid UserId) : IRequest<Unit>;

    internal sealed class DeleteUserHandler(IDbContextFactory<UserDbContext> contextFactory)
        : IHandler<DeleteUserCommand, Unit>
    {
        public async Task<Unit> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

            var user = await context.Users.FindAsync(new object[] { request.UserId }, cancellationToken)
                ?? throw new BussinessException("USER_NOT_FOUND", 404, $"User with ID {request.UserId} not found.");

            context.Users.Remove(user);
            await context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
