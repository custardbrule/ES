using System.Collections.Immutable;

namespace Infras.User.Services.Dtos
{
    public sealed record ApplicationDto(
        string Id,
        string ClientId,
        string DisplayName,
        string ClientType,
        string ConsentType,
        ImmutableArray<string> RedirectUris,
        ImmutableArray<string> PostLogoutRedirectUris,
        ImmutableArray<string> Permissions
    );

    public sealed record ApplicationDetailsDto(
        string Id,
        string ClientId,
        string DisplayName,
        string ClientType,
        string ConsentType,
        ImmutableArray<string> RedirectUris,
        ImmutableArray<string> PostLogoutRedirectUris,
        ImmutableArray<string> Permissions,
        List<ApplicationRoleDto> Roles,
        List<ApplicationScopeDto> Scopes
    );

    public sealed record ApplicationRoleDto(Guid Id, string ApplicationId, string Name, string Description, List<string> Scopes);

    public sealed record ApplicationScopeDto(Guid Id, string ApplicationId, string Name);
}
