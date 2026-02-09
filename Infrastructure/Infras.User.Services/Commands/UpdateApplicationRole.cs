using CQRS;
using Domain.User.ApplicationRoot;
using Microsoft.EntityFrameworkCore;
using Seed;

namespace Infras.User.Services.Commands
{
    public sealed record UpdateApplicationRoleBody(string Name, string Description, List<string>? Scopes);

    public sealed record UpdateApplicationRoleCommand(string ApplicationId, Guid RoleId, UpdateApplicationRoleBody Body)
        : IRequest<Unit>;

    internal sealed class UpdateApplicationRoleHandler(
        IDbContextFactory<UserDbContext> contextFactory)
        : IHandler<UpdateApplicationRoleCommand, Unit>
    {
        public async Task<Unit> Handle(UpdateApplicationRoleCommand request, CancellationToken cancellationToken)
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

            var role = await context.ApplicationRoles
                .Include(r => r.ApplicationRoleScopes)
                .FirstOrDefaultAsync(r => r.Id == request.RoleId && r.ApplicationId == request.ApplicationId, cancellationToken)
                ?? throw new BussinessException("APP_ROLE_NOT_FOUND", 404, "Application role not found.");

            var body = request.Body;

            // Check duplicate name (excluding current role)
            var duplicate = await context.ApplicationRoles
                .AnyAsync(r => r.ApplicationId == request.ApplicationId && r.Name == body.Name && r.Id != request.RoleId, cancellationToken);

            if (duplicate)
                throw new BussinessException("DUPLICATE_APP_ROLE", 409, $"Role '{body.Name}' already exists for this application.");

            // Update via record 'with' won't work well with EF tracking, so use Entry
            context.Entry(role).Property(r => r.Name).CurrentValue = body.Name;
            context.Entry(role).Property(r => r.Description).CurrentValue = body.Description;

            // Replace scopes
            context.ApplicationRoleScopes.RemoveRange(role.ApplicationRoleScopes);
            if (body.Scopes != null)
                context.ApplicationRoleScopes.AddRange(body.Scopes.Select(s => new ApplicationRoleScope(Guid.NewGuid(), role.Id, s)));

            await context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
