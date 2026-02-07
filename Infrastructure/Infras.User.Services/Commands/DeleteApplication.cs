using CQRS;
using OpenIddict.Abstractions;
using Seed;

namespace Infras.User.Services.Commands
{
    public sealed record DeleteApplicationCommand(string Id) : IRequest<Unit>;

    internal sealed class DeleteApplicationHandler(
        IOpenIddictApplicationManager applicationManager)
        : IHandler<DeleteApplicationCommand, Unit>
    {
        public async Task<Unit> Handle(DeleteApplicationCommand request, CancellationToken cancellationToken)
        {
            var app = await applicationManager.FindByIdAsync(request.Id, cancellationToken);
            if (app == null)
            {
                throw new BussinessException("APP_NOT_FOUND", 404, "Application not found.");
            }

            await applicationManager.DeleteAsync(app, cancellationToken);

            return Unit.Value;
        }
    }
}
