using System.Collections.Immutable;
using CQRS;
using OpenIddict.Abstractions;
using Utilities;

namespace Infras.User.Services.Queries
{
    public sealed record GetAllApplicationsQuery(int Page = 1, int PageSize = 10) : IRequest<PagedList<ApplicationDto>>;

    public sealed record ApplicationDto(
        string Id,
        string ClientId,
        string DisplayName,
        string ClientType,
        ImmutableArray<string> RedirectUris,
        ImmutableArray<string> PostLogoutRedirectUris,
        ImmutableArray<string> Permissions
    );

    internal sealed class GetAllApplicationsHandler(
        IOpenIddictApplicationManager applicationManager)
        : IHandler<GetAllApplicationsQuery, PagedList<ApplicationDto>>
    {
        public async Task<PagedList<ApplicationDto>> Handle(GetAllApplicationsQuery request, CancellationToken cancellationToken)
        {
            var page = request.Page < 1 ? 1 : request.Page;
            var pageSize = request.PageSize < 1 ? 10 : request.PageSize > 100 ? 100 : request.PageSize;
            var offset = (page - 1) * pageSize;

            var totalCount = await applicationManager.CountAsync(cancellationToken);

            var applications = new List<ApplicationDto>();
            await foreach (var app in applicationManager.ListAsync(pageSize, offset, cancellationToken))
            {
                applications.Add(new ApplicationDto(
                    Id: await applicationManager.GetIdAsync(app, cancellationToken) ?? string.Empty,
                    ClientId: await applicationManager.GetClientIdAsync(app, cancellationToken) ?? string.Empty,
                    DisplayName: await applicationManager.GetDisplayNameAsync(app, cancellationToken) ?? string.Empty,
                    ClientType: await applicationManager.GetClientTypeAsync(app, cancellationToken) ?? string.Empty,
                    RedirectUris: await applicationManager.GetRedirectUrisAsync(app, cancellationToken),
                    PostLogoutRedirectUris: await applicationManager.GetPostLogoutRedirectUrisAsync(app, cancellationToken),
                    Permissions: await applicationManager.GetPermissionsAsync(app, cancellationToken)
                ));
            }

            return PagedList<ApplicationDto>.Create(applications, (int)totalCount, page, pageSize);
        }
    }
}
