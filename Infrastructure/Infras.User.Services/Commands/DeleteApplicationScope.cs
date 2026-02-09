using CQRS;
using Microsoft.EntityFrameworkCore;
using Seed;

namespace Infras.User.Services.Commands
{
    public sealed record DeleteApplicationScopeCommand(string ApplicationId, Guid ScopeId) : IRequest<Unit>;

    internal sealed class DeleteApplicationScopeHandler(
        IDbContextFactory<UserDbContext> contextFactory)
        : IHandler<DeleteApplicationScopeCommand, Unit>
    {
        public async Task<Unit> Handle(DeleteApplicationScopeCommand request, CancellationToken cancellationToken)
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

            var scope = await context.ApplicationScopes
                .FirstOrDefaultAsync(s => s.Id == request.ScopeId && s.ApplicationId == request.ApplicationId, cancellationToken)
                ?? throw new BussinessException("APP_SCOPE_NOT_FOUND", 404, "Application scope not found.");

            context.ApplicationScopes.Remove(scope);
            await context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
