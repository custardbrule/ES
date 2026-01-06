using CQRS;
using OpenIddict.Abstractions;

namespace Infras.User.Services.Queries
{
    public sealed record GetAllScopesQuery() : IRequest<List<ScopeDto>>;

    public sealed record ScopeDto(
        string Id,
        string Name,
        string DisplayName,
        string? Description
    );

    internal sealed class GetAllScopesHandler(
        IOpenIddictScopeManager scopeManager)
        : IHandler<GetAllScopesQuery, List<ScopeDto>>
    {
        public async Task<List<ScopeDto>> Handle(GetAllScopesQuery request, CancellationToken cancellationToken)
        {
            var scopes = new List<ScopeDto>();

            await foreach (var scope in scopeManager.ListAsync(cancellationToken: cancellationToken))
            {
                scopes.Add(new ScopeDto(
                    Id: await scopeManager.GetIdAsync(scope, cancellationToken) ?? string.Empty,
                    Name: await scopeManager.GetNameAsync(scope, cancellationToken) ?? string.Empty,
                    DisplayName: await scopeManager.GetDisplayNameAsync(scope, cancellationToken) ?? string.Empty,
                    Description: await scopeManager.GetDescriptionAsync(scope, cancellationToken)
                ));
            }

            return scopes;
        }
    }
}
