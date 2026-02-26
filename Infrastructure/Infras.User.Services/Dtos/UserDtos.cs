namespace Infras.User.Services.Dtos
{
    public sealed record UserDto(
        Guid Id,
        string Account,
        DateTimeOffset CreatedDate
    );

    public sealed record UserDetailsDto(
        Guid Id,
        string Account,
        DateTimeOffset CreatedDate,
        List<UserRoleInfo> Roles,
        List<string> Scopes
    );

    public sealed record UserRoleInfo(Guid Id, Guid RoleId, List<string> Scopes);
}
