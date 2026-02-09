using System.Security.Cryptography;
using CQRS;
using OpenIddict.Abstractions;
using RequestValidatior;
using Seed;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Infras.User.Services.Commands
{
    /// <summary>
    /// Creates a new OpenID Connect application with default OIDC permissions.
    /// - Confidential clients: auto-generates a 256-bit client secret
    /// - Public clients: enforces PKCE (Proof Key for Code Exchange)
    /// </summary>
    /// <param name="DisplayName">Human-readable name for the application</param>
    /// <param name="ClientType">Either 'public' or 'confidential' (defaults to 'public')</param>
    /// <param name="RedirectUris">Allowed redirect URIs after authentication</param>
    /// <param name="PostLogoutRedirectUris">Allowed redirect URIs after logout</param>
    public sealed record CreateApplicationCommand(
        string DisplayName,
        string? ClientType,
        List<string>? RedirectUris,
        List<string>? PostLogoutRedirectUris
    ) : IRequest<CreateApplicationResult>;

    /// <param name="Id">The generated application ID</param>
    /// <param name="ClientSecret">The generated secret (only for confidential clients, null for public)</param>
    public sealed record CreateApplicationResult(string Id, string? ClientSecret);

    public sealed class CreateApplicationCommandValidator : BaseValidator<CreateApplicationCommand>
    {
        private readonly HashSet<string> ValidClientTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            ClientTypes.Public,
            ClientTypes.Confidential
        };

        public CreateApplicationCommandValidator()
        {
            RuleFor(x => x.ClientType)
                .With(type => type == null || ValidClientTypes.Contains(type), "ClientType must be 'public' or 'confidential'");

            RuleFor(x => x.RedirectUris)
                .With(uris => uris == null || uris.All(u => IsValidAbsoluteUri(u)), "RedirectUris must be valid absolute URIs");

            RuleFor(x => x.PostLogoutRedirectUris)
                .With(uris => uris == null || uris.All(u => IsValidAbsoluteUri(u)), "PostLogoutRedirectUris must be valid absolute URIs");
        }

        private bool IsValidAbsoluteUri(string uri) => Uri.TryCreate(uri, UriKind.Absolute, out _);
    }

    internal sealed class CreateApplicationHandler(
        IOpenIddictApplicationManager applicationManager)
        : IHandler<CreateApplicationCommand, CreateApplicationResult>
    {
        public async Task<CreateApplicationResult> Handle(CreateApplicationCommand request, CancellationToken cancellationToken)
        {
            var isConfidential = string.Equals(request.ClientType, ClientTypes.Confidential, StringComparison.OrdinalIgnoreCase);

            var descriptor = new OpenIddictApplicationDescriptor
            {
                ClientId = Guid.NewGuid().ToString("N"),
                DisplayName = request.DisplayName,
                ClientType = isConfidential ? ClientTypes.Confidential : ClientTypes.Public
            };

            // Confidential: generate a 256-bit secret for client_credentials / token exchange
            // Public: require PKCE to protect the authorization code flow without a secret
            string? plainSecret = null;
            if (isConfidential)
            {
                plainSecret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
                descriptor.ClientSecret = plainSecret;
            }
            else
            {
                descriptor.Requirements.Add(Requirements.Features.ProofKeyForCodeExchange);
            }

            if (request.RedirectUris != null)
                descriptor.RedirectUris.UnionWith(request.RedirectUris.Select(uri => new Uri(uri)));

            if (request.PostLogoutRedirectUris != null)
                descriptor.PostLogoutRedirectUris.UnionWith(request.PostLogoutRedirectUris.Select(uri => new Uri(uri)));

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

            var app = await applicationManager.CreateAsync(descriptor, cancellationToken);
            var id = await applicationManager.GetIdAsync(app, cancellationToken)
                ?? throw new BussinessException("CREATE_FAILED", 500, "Failed to create application.");

            return new CreateApplicationResult(id, plainSecret);
        }
    }
}
