using CQRS;
using OpenIddict.Abstractions;
using Seed;

namespace Infras.User.Services.Commands
{
    public sealed record DeleteScopeCommand(string Id) : IRequest<Unit>;

    internal sealed class DeleteScopeHandler(
        IOpenIddictScopeManager scopeManager)
        : IHandler<DeleteScopeCommand, Unit>
    {
        public async Task<Unit> Handle(DeleteScopeCommand request, CancellationToken cancellationToken)
        {
            var scope = await scopeManager.FindByIdAsync(request.Id, cancellationToken);
            if (scope == null)
            {
                throw new BussinessException("SCOPE_NOT_FOUND", 404, "Scope not found.");
            }

            await scopeManager.DeleteAsync(scope, cancellationToken);

            return Unit.Value;
        }
    }
}
