using System.Security.Cryptography;
using CQRS;
using OpenIddict.Abstractions;
using RequestValidatior;
using Seed;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Infras.User.Services.Commands
{
    /// <summary>
    /// Updates an existing OpenID Connect application.
    /// - Only updates DisplayName, ClientType, RedirectUris, PostLogoutRedirectUris
    /// - ClientId cannot be changed (immutable identifier)
    /// - Switching to confidential: auto-generates a new 256-bit secret
    /// - Switching to public: clears the secret and enforces PKCE
    /// </summary>
    public sealed record UpdateApplicationBody(
        string DisplayName,
        string? ClientType,
        List<string>? RedirectUris,
        List<string>? PostLogoutRedirectUris
    );

    public sealed record UpdateApplicationCommand(string Id, UpdateApplicationBody Body) : IRequest<UpdateApplicationResult>;

    /// <param name="ClientSecret">The newly generated secret (only when switching to confidential, null otherwise)</param>
    public sealed record UpdateApplicationResult(string? ClientSecret);

    public sealed class UpdateApplicationCommandValidator : BaseValidator<UpdateApplicationCommand>
    {
        private readonly HashSet<string> ValidClientTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            ClientTypes.Public,
            ClientTypes.Confidential
        };

        public UpdateApplicationCommandValidator()
        {
            RuleFor(x => x.Body.ClientType)
                .With(type => type == null || ValidClientTypes.Contains(type), "ClientType must be 'public' or 'confidential'");

            RuleFor(x => x.Body.RedirectUris)
                .With(uris => uris == null || uris.All(u => IsValidAbsoluteUri(u)), "RedirectUris must be valid absolute URIs");

            RuleFor(x => x.Body.PostLogoutRedirectUris)
                .With(uris => uris == null || uris.All(u => IsValidAbsoluteUri(u)), "PostLogoutRedirectUris must be valid absolute URIs");
        }

        private bool IsValidAbsoluteUri(string uri) => Uri.TryCreate(uri, UriKind.Absolute, out _);
    }

    internal sealed class UpdateApplicationHandler(
        IOpenIddictApplicationManager applicationManager)
        : IHandler<UpdateApplicationCommand, UpdateApplicationResult>
    {
        public async Task<UpdateApplicationResult> Handle(UpdateApplicationCommand request, CancellationToken cancellationToken)
        {
            var app = await applicationManager.FindByIdAsync(request.Id, cancellationToken)
                ?? throw new BussinessException("APP_NOT_FOUND", 404, "Application not found.");

            var body = request.Body;
            var isConfidential = string.Equals(body.ClientType, ClientTypes.Confidential, StringComparison.OrdinalIgnoreCase);

            var descriptor = new OpenIddictApplicationDescriptor();
            await applicationManager.PopulateAsync(descriptor, app, cancellationToken);

            var wasConfidential = string.Equals(descriptor.ClientType, ClientTypes.Confidential, StringComparison.OrdinalIgnoreCase);

            // Update mutable fields
            descriptor.DisplayName = body.DisplayName;
            descriptor.ClientType = isConfidential ? ClientTypes.Confidential : ClientTypes.Public;

            // Handle secret based on client type change
            string? plainSecret = null;
            if (isConfidential && !wasConfidential)
            {
                // Public → Confidential: generate a new secret
                plainSecret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
                descriptor.ClientSecret = plainSecret;
            }
            else if (!isConfidential && wasConfidential)
            {
                // Confidential → Public: clear the secret
                descriptor.ClientSecret = null;
            }

            // Replace URIs
            descriptor.RedirectUris.Clear();
            if (body.RedirectUris != null)
                descriptor.RedirectUris.UnionWith(body.RedirectUris.Select(uri => new Uri(uri)));

            descriptor.PostLogoutRedirectUris.Clear();
            if (body.PostLogoutRedirectUris != null)
                descriptor.PostLogoutRedirectUris.UnionWith(body.PostLogoutRedirectUris.Select(uri => new Uri(uri)));

            // Rebuild permissions & requirements based on client type
            descriptor.Permissions.Clear();
            descriptor.Requirements.Clear();

            if (!isConfidential)
                descriptor.Requirements.Add(Requirements.Features.ProofKeyForCodeExchange);

            // Default OIDC permissions:
            // Endpoints  – authorize, token, logout
            // Grants     – authorization_code + refresh_token
            // Response   – code (for authorization code flow)
            // Scopes     – openid, email, profile, roles
            descriptor.Permissions.Add(Permissions.Endpoints.Authorization);
            descriptor.Permissions.Add(Permissions.Endpoints.Token);
            descriptor.Permissions.Add(Permissions.Endpoints.Logout);
            descriptor.Permissions.Add(Permissions.GrantTypes.AuthorizationCode);
            descriptor.Permissions.Add(Permissions.GrantTypes.RefreshToken);
            descriptor.Permissions.Add(Permissions.ResponseTypes.Code);
            descriptor.Permissions.Add(Permissions.Scopes.Email);
            descriptor.Permissions.Add(Permissions.Scopes.Profile);
            descriptor.Permissions.Add(Permissions.Scopes.Roles);
            descriptor.Permissions.Add($"{Permissions.Prefixes.Scope}{Scopes.OpenId}");

            await applicationManager.UpdateAsync(app, descriptor, cancellationToken);

            return new UpdateApplicationResult(plainSecret);
        }
    }
}
