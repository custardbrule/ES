using CQRS;
using OpenIddict.Abstractions;

namespace Infras.User.Services.Queries
{
    public sealed record GetApplicationByIdQuery(string Id) : IRequest<ApplicationDetailsDto>;

    public sealed record ApplicationDetailsDto(
        string Id,
        string ClientId,
        string DisplayName,
        string Type,
        string ConsentType,
        List<string> RedirectUris,
        List<string> PostLogoutRedirectUris,
        List<string> Permissions
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
                throw new InvalidOperationException("Application not found.");
            }

            return new ApplicationDetailsDto(
                Id: await applicationManager.GetIdAsync(app, cancellationToken) ?? string.Empty,
                ClientId: await applicationManager.GetClientIdAsync(app, cancellationToken) ?? string.Empty,
                DisplayName: await applicationManager.GetDisplayNameAsync(app, cancellationToken) ?? string.Empty,
                Type: await applicationManager.GetClientTypeAsync(app, cancellationToken) ?? string.Empty,
                ConsentType: await applicationManager.GetConsentTypeAsync(app, cancellationToken) ?? string.Empty,
                RedirectUris: (await applicationManager.GetRedirectUrisAsync(app, cancellationToken)).ToList(),
                PostLogoutRedirectUris: (await applicationManager.GetPostLogoutRedirectUrisAsync(app, cancellationToken)).ToList(),
                Permissions: (await applicationManager.GetPermissionsAsync(app, cancellationToken)).ToList()
            );
        }
    }
}
