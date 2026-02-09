using CQRS;
using Domain.User.ApplicationRoot;
using Microsoft.EntityFrameworkCore;

namespace Infras.User.Services.Commands
{
    public sealed record ReplaceApplicationScopesCommand(string ApplicationId, List<string> Names) : IRequest<Unit>;

    internal sealed class ReplaceApplicationScopesHandler(
        IDbContextFactory<UserDbContext> contextFactory)
        : IHandler<ReplaceApplicationScopesCommand, Unit>
    {
        public async Task<Unit> Handle(ReplaceApplicationScopesCommand request, CancellationToken cancellationToken)
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

            await context.ApplicationScopes
                .Where(s => s.ApplicationId == request.ApplicationId)
                .ExecuteDeleteAsync(cancellationToken);

            if (request.Names.Count > 0)
            {
                var distinct = request.Names.Distinct().ToList();
                context.ApplicationScopes.AddRange(distinct.Select(name =>
                    new ApplicationScope(Guid.NewGuid(), request.ApplicationId, name)));
                await context.SaveChangesAsync(cancellationToken);
            }

            return Unit.Value;
        }
    }
}
