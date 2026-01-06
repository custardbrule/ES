using CQRS;
using OpenIddict.Abstractions;

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
                throw new InvalidOperationException("Scope not found.");
            }

            await scopeManager.DeleteAsync(scope, cancellationToken);

            return Unit.Value;
        }
    }
}
