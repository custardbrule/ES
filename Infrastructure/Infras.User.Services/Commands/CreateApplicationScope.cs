using CQRS;
using Domain.User.ApplicationRoot;
using Microsoft.EntityFrameworkCore;
using Seed;

namespace Infras.User.Services.Commands
{
    public sealed record CreateApplicationScopeCommand(string ApplicationId, string ScopeId) : IRequest<Guid>;

    internal sealed class CreateApplicationScopeHandler(
        IDbContextFactory<UserDbContext> contextFactory)
        : IHandler<CreateApplicationScopeCommand, Guid>
    {
        public async Task<Guid> Handle(CreateApplicationScopeCommand request, CancellationToken cancellationToken)
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

            var exists = await context.ApplicationScopes
                .AnyAsync(s => s.ApplicationId == request.ApplicationId && s.ScopeId == request.ScopeId, cancellationToken);

            if (exists)
                throw new BussinessException("DUPLICATE_APP_SCOPE", 409, $"Scope '{request.ScopeId}' is already assigned to this application.");

            var scope = new ApplicationScope(Guid.NewGuid(), request.ApplicationId, request.ScopeId);

            context.ApplicationScopes.Add(scope);
            await context.SaveChangesAsync(cancellationToken);

            return scope.Id;
        }
    }
}
