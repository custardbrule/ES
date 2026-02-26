using CQRS;
using Infras.User.Services.Dtos;
using OpenIddict.Abstractions;
using RequestValidatior;
using Utilities;

namespace Infras.User.Services.Queries
{
    public sealed record GetAllApplicationsQuery(int Page = 1, int PageSize = 10) : IRequest<PagedList<ApplicationDto>>;

    public sealed class GetAllApplicationsQueryValidator : BaseValidator<GetAllApplicationsQuery>
    {
        public GetAllApplicationsQueryValidator()
        {
            RuleFor(x => x.Page)
                .With(p => p >= 1, "Page must be at least 1.");
            RuleFor(x => x.PageSize)
                .With(s => s >= 10, "PageSize must be at least 10.")
                .With(s => s <= 100, "PageSize must not exceed 100.");
        }
    }

    internal sealed class GetAllApplicationsHandler(
        IOpenIddictApplicationManager applicationManager)
        : IHandler<GetAllApplicationsQuery, PagedList<ApplicationDto>>
    {
        public async Task<PagedList<ApplicationDto>> Handle(GetAllApplicationsQuery request, CancellationToken cancellationToken)
        {
            var page = request.Page;
            var pageSize = request.PageSize;
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
                    ConsentType: await applicationManager.GetConsentTypeAsync(app, cancellationToken) ?? string.Empty,
                    RedirectUris: await applicationManager.GetRedirectUrisAsync(app, cancellationToken),
                    PostLogoutRedirectUris: await applicationManager.GetPostLogoutRedirectUrisAsync(app, cancellationToken),
                    Permissions: await applicationManager.GetPermissionsAsync(app, cancellationToken)
                ));
            }

            return PagedList<ApplicationDto>.Create(applications, (int)totalCount, page, pageSize);
        }
    }
}
