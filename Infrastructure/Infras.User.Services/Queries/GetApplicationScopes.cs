using CQRS;
using Infras.User.Services.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Infras.User.Services.Queries
{
    public sealed record GetApplicationScopesQuery(string ApplicationId) : IRequest<List<ApplicationScopeDto>>;

    internal sealed class GetApplicationScopesHandler(
        IDbContextFactory<UserDbContext> contextFactory)
        : IHandler<GetApplicationScopesQuery, List<ApplicationScopeDto>>
    {
        public async Task<List<ApplicationScopeDto>> Handle(GetApplicationScopesQuery request, CancellationToken cancellationToken)
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

            return await context.ApplicationScopes
                .Where(s => s.ApplicationId == request.ApplicationId)
                .Select(s => new ApplicationScopeDto(s.Id, s.ApplicationId, s.Name))
                .ToListAsync(cancellationToken);
        }
    }
}
