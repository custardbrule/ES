using CQRS;
using Microsoft.EntityFrameworkCore;
using Seed;
using Utilities;

namespace Infras.User.Services.Commands
{
    public sealed record AssignRoleToUserCommand(Guid UserId, Guid RoleId) : IRequest<Unit>;

    internal sealed class AssignRoleToUserHandler(IDbContextFactory<UserDbContext> contextFactory)
        : IHandler<AssignRoleToUserCommand, Unit>
    {
        public async Task<Unit> Handle(AssignRoleToUserCommand request, CancellationToken cancellationToken)
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

            var user = await context.Users
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken)
                ?? throw new BussinessException("USER_NOT_FOUND", 404, $"User with ID {request.UserId} not found.");

            var roleExists = await context.Roles.AnyAsync(r => r.Id == request.RoleId, cancellationToken);
            if (!roleExists)
                throw new BussinessException("ROLE_NOT_FOUND", 404, $"Role with ID {request.RoleId} not found.");

            var alreadyAssigned = user.UserRoles.Any(ur => ur.RoleId == request.RoleId);
            if (alreadyAssigned)
                throw new BussinessException("ROLE_ALREADY_ASSIGNED", 409, "Role is already assigned to this user.");

            user.AddUserRole(request.RoleId);
            await context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
