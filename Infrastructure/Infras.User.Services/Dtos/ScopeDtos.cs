namespace Infras.User.Services.Dtos
{
    public sealed record ScopeDto(
        string Id,
        string Name,
        string DisplayName,
        string? Description
    );

    public sealed record ScopeDetailsDto(
        string Id,
        string Name,
        string DisplayName,
        string? Description,
        List<string> Resources
    );
}
