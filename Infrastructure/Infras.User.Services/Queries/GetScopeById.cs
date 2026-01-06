using CQRS;
using OpenIddict.Abstractions;

namespace Infras.User.Services.Queries
{
    public sealed record GetScopeByIdQuery(string Id) : IRequest<ScopeDetailsDto>;

    public sealed record ScopeDetailsDto(
        string Id,
        string Name,
        string DisplayName,
        string? Description,
        List<string> Resources
    );

    internal sealed class GetScopeByIdHandler(
        IOpenIddictScopeManager scopeManager)
        : IHandler<GetScopeByIdQuery, ScopeDetailsDto>
    {
        public async Task<ScopeDetailsDto> Handle(GetScopeByIdQuery request, CancellationToken cancellationToken)
        {
            var scope = await scopeManager.FindByIdAsync(request.Id, cancellationToken);
            if (scope == null)
            {
                throw new InvalidOperationException("Scope not found.");
            }

            return new ScopeDetailsDto(
                Id: await scopeManager.GetIdAsync(scope, cancellationToken) ?? string.Empty,
                Name: await scopeManager.GetNameAsync(scope, cancellationToken) ?? string.Empty,
                DisplayName: await scopeManager.GetDisplayNameAsync(scope, cancellationToken) ?? string.Empty,
                Description: await scopeManager.GetDescriptionAsync(scope, cancellationToken),
                Resources: (await scopeManager.GetResourcesAsync(scope, cancellationToken)).ToList()
            );
        }
    }
}
