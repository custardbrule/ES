using CQRS;
using Infras.User.Services.Dtos;
using Microsoft.EntityFrameworkCore;
using Seed;

namespace Infras.User.Services.Queries
{
    public sealed record GetUserDetailsQuery(Guid UserId) : IRequest<UserDetailsDto>;

    internal sealed class GetUserDetailsHandler(IDbContextFactory<UserDbContext> contextFactory)
        : IHandler<GetUserDetailsQuery, UserDetailsDto>
    {
        public async Task<UserDetailsDto> Handle(GetUserDetailsQuery request, CancellationToken cancellationToken)
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

            var user = await context.Users
                .Include(u => u.UserRoles)
                .Include(u => u.Scopes)
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken)
                ?? throw new BussinessException("USER_NOT_FOUND", 404, $"User with ID {request.UserId} not found.");

            var roleIds = user.UserRoles.Select(ur => ur.RoleId).ToList();
            var roleScopes = await context.RoleScopes
                .Where(rs => roleIds.Contains(rs.RoleId))
                .GroupBy(rs => rs.RoleId)
                .ToDictionaryAsync(g => g.Key, g => g.Select(rs => rs.Scope).ToList(), cancellationToken);

            return new UserDetailsDto(
                Id: user.Id,
                Account: user.Account,
                CreatedDate: user.CreatedDate,
                Roles: user.UserRoles.Select(r => new UserRoleInfo(
                    r.Id,
                    r.RoleId,
                    roleScopes.TryGetValue(r.RoleId, out var scopes) ? scopes : []
                )).ToList(),
                Scopes: user.Scopes.Select(s => s.Scope).ToList()
            );
        }
    }
}
