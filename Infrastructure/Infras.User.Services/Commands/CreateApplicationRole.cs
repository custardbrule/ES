using CQRS;
using Domain.User.ApplicationRoot;
using Microsoft.EntityFrameworkCore;
using Seed;

namespace Infras.User.Services.Commands
{
    public sealed record CreateApplicationRoleBody(string Name, string Description, List<string>? Scopes);

    public sealed record CreateApplicationRoleCommand(string ApplicationId, CreateApplicationRoleBody Body) : IRequest<Guid>;

    internal sealed class CreateApplicationRoleHandler(
        IDbContextFactory<UserDbContext> contextFactory)
        : IHandler<CreateApplicationRoleCommand, Guid>
    {
        public async Task<Guid> Handle(CreateApplicationRoleCommand request, CancellationToken cancellationToken)
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

            var body = request.Body;

            var exists = await context.ApplicationRoles
                .AnyAsync(r => r.ApplicationId == request.ApplicationId && r.Name == body.Name, cancellationToken);

            if (exists)
                throw new BussinessException("DUPLICATE_APP_ROLE", 409, $"Role '{body.Name}' already exists for this application.");

            var role = new ApplicationRole(Guid.NewGuid(), request.ApplicationId, body.Name, body.Description);

            if (body.Scopes != null)
            {
                foreach (var scope in body.Scopes)
                    role.ApplicationRoleScopes.Add(new ApplicationRoleScope(Guid.NewGuid(), role.Id, scope));
            }

            context.ApplicationRoles.Add(role);
            await context.SaveChangesAsync(cancellationToken);

            return role.Id;
        }
    }
}
