using CQRS;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Infras.User.Services.Commands
{
    public sealed record CreateApplicationCommand(
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
    ) : IRequest<string>;

    internal sealed class CreateApplicationHandler(
        IOpenIddictApplicationManager applicationManager)
        : IHandler<CreateApplicationCommand, string>
    {
        public async Task<string> Handle(CreateApplicationCommand request, CancellationToken cancellationToken)
        {
            // Check if client already exists
            if (await applicationManager.FindByClientIdAsync(request.ClientId, cancellationToken) != null)
            {
                throw new InvalidOperationException("A client with this ID already exists.");
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

            var app = await applicationManager.CreateAsync(descriptor, cancellationToken);
            var id = await applicationManager.GetIdAsync(app, cancellationToken);

            return id ?? throw new InvalidOperationException("Failed to create application.");
        }
    }
}
