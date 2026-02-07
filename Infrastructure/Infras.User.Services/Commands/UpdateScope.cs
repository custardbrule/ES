using CQRS;
using OpenIddict.Abstractions;
using Seed;

namespace Infras.User.Services.Commands
{
    public sealed record UpdateScopeCommand(
        string Id,
        string Name,
        string DisplayName,
        string? Description,
        List<string>? Resources
    ) : IRequest<Unit>;

    internal sealed class UpdateScopeHandler(
        IOpenIddictScopeManager scopeManager)
        : IHandler<UpdateScopeCommand, Unit>
    {
        public async Task<Unit> Handle(UpdateScopeCommand request, CancellationToken cancellationToken)
        {
            var scope = await scopeManager.FindByIdAsync(request.Id, cancellationToken);
            if (scope == null)
            {
                throw new BussinessException("SCOPE_NOT_FOUND", 404, "Scope not found.");
            }

            var descriptor = new OpenIddictScopeDescriptor
            {
                Name = request.Name,
                DisplayName = request.DisplayName,
                Description = request.Description
            };

            // Add resources
            if (request.Resources != null)
            {
                foreach (var resource in request.Resources)
                {
                    descriptor.Resources.Add(resource);
                }
            }

            await scopeManager.UpdateAsync(scope, descriptor, cancellationToken);

            return Unit.Value;
        }
    }
}
