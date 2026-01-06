using CQRS;
using OpenIddict.Abstractions;

namespace Infras.User.Services.Commands
{
    public sealed record CreateScopeCommand(
        string Name,
        string DisplayName,
        string? Description,
        List<string>? Resources
    ) : IRequest<string>;

    internal sealed class CreateScopeHandler(
        IOpenIddictScopeManager scopeManager)
        : IHandler<CreateScopeCommand, string>
    {
        public async Task<string> Handle(CreateScopeCommand request, CancellationToken cancellationToken)
        {
            // Check if scope already exists
            if (await scopeManager.FindByNameAsync(request.Name, cancellationToken) != null)
            {
                throw new InvalidOperationException("A scope with this name already exists.");
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

            var scope = await scopeManager.CreateAsync(descriptor, cancellationToken);
            var id = await scopeManager.GetIdAsync(scope, cancellationToken);

            return id ?? throw new InvalidOperationException("Failed to create scope.");
        }
    }
}
