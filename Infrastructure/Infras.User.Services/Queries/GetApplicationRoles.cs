using CQRS;
using Infras.User.Services.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Infras.User.Services.Queries
{
    public sealed record GetApplicationRolesQuery(string ApplicationId) : IRequest<List<ApplicationRoleDto>>;

    internal sealed class GetApplicationRolesHandler(
        IDbContextFactory<UserDbContext> contextFactory)
        : IHandler<GetApplicationRolesQuery, List<ApplicationRoleDto>>
    {
        public async Task<List<ApplicationRoleDto>> Handle(GetApplicationRolesQuery request, CancellationToken cancellationToken)
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

            return await context.ApplicationRoles
                .Where(r => r.ApplicationId == request.ApplicationId)
                .OrderBy(r => r.Name)
                .Select(r => new ApplicationRoleDto(
                    r.Id,
                    r.ApplicationId,
                    r.Name,
                    r.Description,
                    r.ApplicationRoleScopes.Select(s => s.Scope).ToList()
                ))
                .ToListAsync(cancellationToken);
        }
    }
}
