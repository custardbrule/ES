using System.Security.Cryptography;
using CQRS;
using OpenIddict.Abstractions;
using RequestValidatior;
using Seed;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Infras.User.Services.Commands
{
    public sealed record CreateApplicationCommand(
        string ClientId,
        string DisplayName,
        string? ClientType,
        List<string>? RedirectUris,
        List<string>? PostLogoutRedirectUris
    ) : IRequest<CreateApplicationResult>;

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
            if (await applicationManager.FindByClientIdAsync(request.ClientId, cancellationToken) != null)
            {
                throw new BussinessException("DUPLICATE_CLIENT", 409, "A client with this ID already exists.");
            }

            var isConfidential = string.Equals(request.ClientType, ClientTypes.Confidential, StringComparison.OrdinalIgnoreCase);

            var descriptor = new OpenIddictApplicationDescriptor
            {
                ClientId = request.ClientId,
                DisplayName = request.DisplayName,
                ClientType = isConfidential ? ClientTypes.Confidential : ClientTypes.Public
            };

            string? plainSecret = null;
            if (isConfidential)
            {
                plainSecret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
                descriptor.ClientSecret = plainSecret;
            }

            if (request.RedirectUris != null)
                descriptor.RedirectUris.UnionWith(request.RedirectUris.Select(uri => new Uri(uri)));

            if (request.PostLogoutRedirectUris != null)
                descriptor.PostLogoutRedirectUris.UnionWith(request.PostLogoutRedirectUris.Select(uri => new Uri(uri)));

            // Default OIDC permissions
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
