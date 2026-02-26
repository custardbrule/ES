using CQRS;
using Infras.User.Services.Dtos;
using OpenIddict.Abstractions;
using RequestValidatior;
using Utilities;

namespace Infras.User.Services.Queries
{
    public sealed record GetAllScopesQuery(int Page = 1, int PageSize = 10) : IRequest<PagedList<ScopeDto>>;

    public sealed class GetAllScopesQueryValidator : BaseValidator<GetAllScopesQuery>
    {
        public GetAllScopesQueryValidator()
        {
            RuleFor(x => x.Page)
                .With(p => p >= 1, "Page must be at least 1.");
            RuleFor(x => x.PageSize)
                .With(s => s >= 10, "PageSize must be at least 10.")
                .With(s => s <= 100, "PageSize must not exceed 100.");
        }
    }

    internal sealed class GetAllScopesHandler(
        IOpenIddictScopeManager scopeManager)
        : IHandler<GetAllScopesQuery, PagedList<ScopeDto>>
    {
        public async Task<PagedList<ScopeDto>> Handle(GetAllScopesQuery request, CancellationToken cancellationToken)
        {
            var page = request.Page;
            var pageSize = request.PageSize;
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
