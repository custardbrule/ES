using CQRS;
using OpenIddict.Abstractions;
using Utilities;

namespace Infras.User.Services.Queries
{
    public sealed record GetAllScopesQuery(int Page = 1, int PageSize = 10) : IRequest<PagedList<ScopeDto>>;

    public sealed record ScopeDto(
        string Id,
        string Name,
        string DisplayName,
        string? Description
    );

    internal sealed class GetAllScopesHandler(
        IOpenIddictScopeManager scopeManager)
        : IHandler<GetAllScopesQuery, PagedList<ScopeDto>>
    {
        public async Task<PagedList<ScopeDto>> Handle(GetAllScopesQuery request, CancellationToken cancellationToken)
        {
            var page = request.Page < 1 ? 1 : request.Page;
            var pageSize = request.PageSize < 1 ? 10 : request.PageSize > 100 ? 100 : request.PageSize;
            var offset = (page - 1) * pageSize;

            var totalCount = await scopeManager.CountAsync(cancellationToken);

            var scopes = await scopeManager.ListAsync(pageSize, offset, cancellationToken)
                .SelectAwaitWithCancellation(async (scope, ct) => new ScopeDto(
                    Id: await scopeManager.GetIdAsync(scope, ct) ?? string.Empty,
                    Name: await scopeManager.GetNameAsync(scope, ct) ?? string.Empty,
                    DisplayName: await scopeManager.GetDisplayNameAsync(scope, ct) ?? string.Empty,
                    Description: await scopeManager.GetDescriptionAsync(scope, ct)
                ))
                .ToListAsync(cancellationToken);

            return PagedList<ScopeDto>.Create(scopes, (int)totalCount, page, pageSize);
        }
    }
}
