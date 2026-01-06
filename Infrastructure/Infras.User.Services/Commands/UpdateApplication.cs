using CQRS;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Infras.User.Services.Commands
{
    public sealed record UpdateApplicationCommand(
        string Id,
        string ClientId,
        string ClientSecret,
        string DisplayName,
        string? Type,
        List<string>? RedirectUris,
        List<string>? PostLogoutRedirectUris,
        bool AllowRefreshToken,
        bool RequirePkce,
        bool AllowOpenId,
        bool AllowEmail,
        bool AllowProfile,
        bool AllowRoles
    ) : IRequest<Unit>;

    internal sealed class UpdateApplicationHandler(
        IOpenIddictApplicationManager applicationManager)
        : IHandler<UpdateApplicationCommand, Unit>
    {
        public async Task<Unit> Handle(UpdateApplicationCommand request, CancellationToken cancellationToken)
        {
            var app = await applicationManager.FindByIdAsync(request.Id, cancellationToken);
            if (app == null)
            {
                throw new InvalidOperationException("Application not found.");
            }

            var descriptor = new OpenIddictApplicationDescriptor
            {
                ClientId = request.ClientId,
                ClientSecret = request.ClientSecret,
                DisplayName = request.DisplayName,
                ClientType = request.Type ?? ClientTypes.Confidential
            };

            // Add redirect URIs
            if (request.RedirectUris != null)
            {
                foreach (var uri in request.RedirectUris)
                {
                    descriptor.RedirectUris.Add(new Uri(uri));
                }
            }

            // Add post logout redirect URIs
            if (request.PostLogoutRedirectUris != null)
            {
                foreach (var uri in request.PostLogoutRedirectUris)
                {
                    descriptor.PostLogoutRedirectUris.Add(new Uri(uri));
                }
            }

            // Add permissions
            descriptor.Permissions.Add(Permissions.Endpoints.Authorization);
            descriptor.Permissions.Add(Permissions.Endpoints.Token);
            descriptor.Permissions.Add(Permissions.GrantTypes.AuthorizationCode);
            descriptor.Permissions.Add(Permissions.ResponseTypes.Code);

            if (request.AllowRefreshToken)
            {
                descriptor.Permissions.Add(Permissions.GrantTypes.RefreshToken);
            }

            if (request.RequirePkce)
            {
                descriptor.Permissions.Add(Requirements.Features.ProofKeyForCodeExchange);
            }

            // Add scopes
            if (request.AllowOpenId)
            {
                descriptor.Permissions.Add($"{Permissions.Prefixes.Scope}{Scopes.OpenId}");
            }

            if (request.AllowEmail)
            {
                descriptor.Permissions.Add(Permissions.Scopes.Email);
            }

            if (request.AllowProfile)
            {
                descriptor.Permissions.Add(Permissions.Scopes.Profile);
            }

            if (request.AllowRoles)
            {
                descriptor.Permissions.Add(Permissions.Scopes.Roles);
            }

            await applicationManager.UpdateAsync(app, descriptor, cancellationToken);

            return Unit.Value;
        }
    }
}
