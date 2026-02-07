using System.Collections.Immutable;
using CQRS;
using OpenIddict.Abstractions;
using Seed;

namespace Infras.User.Services.Queries
{
    public sealed record GetApplicationByIdQuery(string Id) : IRequest<ApplicationDetailsDto>;

    public sealed record ApplicationDetailsDto(
        string Id,
        string ClientId,
        string DisplayName,
        string ClientType,
        string ConsentType,
        ImmutableArray<string> RedirectUris,
        ImmutableArray<string> PostLogoutRedirectUris,
        ImmutableArray<string> Permissions
    );

    internal sealed class GetApplicationByIdHandler(
        IOpenIddictApplicationManager applicationManager)
        : IHandler<GetApplicationByIdQuery, ApplicationDetailsDto>
    {
        public async Task<ApplicationDetailsDto> Handle(GetApplicationByIdQuery request, CancellationToken cancellationToken)
        {
            var app = await applicationManager.FindByIdAsync(request.Id, cancellationToken);
            if (app == null)
            {
                throw new BussinessException("APP_NOT_FOUND", 404, "Application not found.");
            }

            return new ApplicationDetailsDto(
                Id: await applicationManager.GetIdAsync(app, cancellationToken) ?? string.Empty,
                ClientId: await applicationManager.GetClientIdAsync(app, cancellationToken) ?? string.Empty,
                DisplayName: await applicationManager.GetDisplayNameAsync(app, cancellationToken) ?? string.Empty,
                ClientType: await applicationManager.GetClientTypeAsync(app, cancellationToken) ?? string.Empty,
                ConsentType: await applicationManager.GetConsentTypeAsync(app, cancellationToken) ?? string.Empty,
                RedirectUris: await applicationManager.GetRedirectUrisAsync(app, cancellationToken),
                PostLogoutRedirectUris: await applicationManager.GetPostLogoutRedirectUrisAsync(app, cancellationToken),
                Permissions: await applicationManager.GetPermissionsAsync(app, cancellationToken)
            );
        }
    }
}
