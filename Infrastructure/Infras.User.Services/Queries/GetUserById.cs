using CQRS;
using Microsoft.EntityFrameworkCore;

namespace Infras.User.Services.Queries
{
    public sealed record GetUserByIdQuery(Guid UserId) : IRequest<Domain.User.UserRoot.User>;

    internal sealed class GetUserByIdHandler(IDbContextFactory<UserDbContext> contextFactory)
        : IHandler<GetUserByIdQuery, Domain.User.UserRoot.User>
    {
        public async Task<Domain.User.UserRoot.User> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

            var user = await context.Users.FindAsync(new object[] { request.UserId }, cancellationToken);

            if (user == null)
                throw new InvalidOperationException($"User with ID {request.UserId} not found.");

            return user;
        }
    }
}
