using CQRS;
using Microsoft.EntityFrameworkCore;

namespace Infras.User.Services.Queries
{
    public sealed record GetUserWithRelationsQuery(Guid UserId) : IRequest<Domain.User.UserRoot.User>;

    internal sealed class GetUserWithRelationsHandler(IDbContextFactory<UserDbContext> contextFactory)
        : IHandler<GetUserWithRelationsQuery, Domain.User.UserRoot.User>
    {
        public async Task<Domain.User.UserRoot.User> Handle(GetUserWithRelationsQuery request, CancellationToken cancellationToken)
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

            var user = await context.Users
                .Include(u => u.UserRoles)
                .Include(u => u.Scopes)
                .Include(u => u.LoginHistories)
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user == null)
                throw new InvalidOperationException($"User with ID {request.UserId} not found.");

            return user;
        }
    }
}
