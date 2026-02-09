using CQRS;
using Microsoft.EntityFrameworkCore;
using Seed;

namespace Infras.User.Services.Commands
{
    public sealed record DeleteApplicationRoleCommand(string ApplicationId, Guid RoleId) : IRequest<Unit>;

    internal sealed class DeleteApplicationRoleHandler(
        IDbContextFactory<UserDbContext> contextFactory)
        : IHandler<DeleteApplicationRoleCommand, Unit>
    {
        public async Task<Unit> Handle(DeleteApplicationRoleCommand request, CancellationToken cancellationToken)
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

            var role = await context.ApplicationRoles
                .FirstOrDefaultAsync(r => r.Id == request.RoleId && r.ApplicationId == request.ApplicationId, cancellationToken)
                ?? throw new BussinessException("APP_ROLE_NOT_FOUND", 404, "Application role not found.");

            // ApplicationRoleScopes cascade-deleted by FK constraint
            context.ApplicationRoles.Remove(role);
            await context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
