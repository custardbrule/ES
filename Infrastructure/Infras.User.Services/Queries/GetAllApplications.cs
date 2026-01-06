using CQRS;
using OpenIddict.Abstractions;

namespace Infras.User.Services.Queries
{
    public sealed record GetAllApplicationsQuery() : IRequest<List<ApplicationDto>>;

    public sealed record ApplicationDto(
        string Id,
        string ClientId,
        string DisplayName,
        string Type
    );

    internal sealed class GetAllApplicationsHandler(
        IOpenIddictApplicationManager applicationManager)
        : IHandler<GetAllApplicationsQuery, List<ApplicationDto>>
    {
        public async Task<List<ApplicationDto>> Handle(GetAllApplicationsQuery request, CancellationToken cancellationToken)
        {
            var applications = new List<ApplicationDto>();

            await foreach (var app in applicationManager.ListAsync(cancellationToken: cancellationToken))
            {
                applications.Add(new ApplicationDto(
                    Id: await applicationManager.GetIdAsync(app, cancellationToken) ?? string.Empty,
                    ClientId: await applicationManager.GetClientIdAsync(app, cancellationToken) ?? string.Empty,
                    DisplayName: await applicationManager.GetDisplayNameAsync(app, cancellationToken) ?? string.Empty,
                    Type: await applicationManager.GetClientTypeAsync(app, cancellationToken) ?? string.Empty
                ));
            }

            return applications;
        }
    }
}
