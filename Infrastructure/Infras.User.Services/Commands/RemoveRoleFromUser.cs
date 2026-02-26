using CQRS;
using Microsoft.EntityFrameworkCore;
using Seed;
using Utilities;

namespace Infras.User.Services.Commands
{
    public sealed record RemoveRoleFromUserCommand(Guid UserId, Guid RoleId) : IRequest<Unit>;

    internal sealed class RemoveRoleFromUserHandler(IDbContextFactory<UserDbContext> contextFactory)
        : IHandler<RemoveRoleFromUserCommand, Unit>
    {
        public async Task<Unit> Handle(RemoveRoleFromUserCommand request, CancellationToken cancellationToken)
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

            var user = await context.Users
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken)
                ?? throw new BussinessException("USER_NOT_FOUND", 404, $"User with ID {request.UserId} not found.");

            var removed = user.RemoveUserRole(request.RoleId);
            if (!removed)
                throw new BussinessException("ROLE_NOT_ASSIGNED", 404, "Role is not assigned to this user.");

            await context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
