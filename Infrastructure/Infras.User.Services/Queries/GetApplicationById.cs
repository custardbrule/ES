using CQRS;
using Infras.User.Services.Dtos;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using Seed;

namespace Infras.User.Services.Queries
{
    public sealed record GetApplicationByIdQuery(string Id) : IRequest<ApplicationDetailsDto>;

    internal sealed class GetApplicationByIdHandler(
        IOpenIddictApplicationManager applicationManager,
        IDbContextFactory<UserDbContext> contextFactory)
        : IHandler<GetApplicationByIdQuery, ApplicationDetailsDto>
    {
        public async Task<ApplicationDetailsDto> Handle(GetApplicationByIdQuery request, CancellationToken cancellationToken)
        {
            var app = await applicationManager.FindByIdAsync(request.Id, cancellationToken)
                ?? throw new BussinessException("APP_NOT_FOUND", 404, "Application not found.");

            var appId = await applicationManager.GetIdAsync(app, cancellationToken) ?? string.Empty;

            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

            var roles = await context.ApplicationRoles
                .Where(r => r.ApplicationId == appId)
                .OrderBy(r => r.Name)
                .Select(r => new ApplicationRoleDto(
                    r.Id,
                    r.ApplicationId,
                    r.Name,
                    r.Description,
                    r.ApplicationRoleScopes.Select(s => s.Scope).ToList()
                ))
                .ToListAsync(cancellationToken);

            var scopes = await context.ApplicationScopes
                .Where(s => s.ApplicationId == appId)
                .Select(s => new ApplicationScopeDto(s.Id, s.ApplicationId, s.Name))
                .ToListAsync(cancellationToken);

            return new ApplicationDetailsDto(
                Id: appId,
                ClientId: await applicationManager.GetClientIdAsync(app, cancellationToken) ?? string.Empty,
                DisplayName: await applicationManager.GetDisplayNameAsync(app, cancellationToken) ?? string.Empty,
                ClientType: await applicationManager.GetClientTypeAsync(app, cancellationToken) ?? string.Empty,
                ConsentType: await applicationManager.GetConsentTypeAsync(app, cancellationToken) ?? string.Empty,
                RedirectUris: await applicationManager.GetRedirectUrisAsync(app, cancellationToken),
                PostLogoutRedirectUris: await applicationManager.GetPostLogoutRedirectUrisAsync(app, cancellationToken),
                Permissions: await applicationManager.GetPermissionsAsync(app, cancellationToken),
                Roles: roles,
                Scopes: scopes
            );
        }
    }
}
