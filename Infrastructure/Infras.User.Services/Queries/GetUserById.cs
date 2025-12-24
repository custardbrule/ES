using CQRS;

namespace Infras.User.Services.Queries
{
    public sealed record GetUserByIdQuery(Guid UserId) : IRequest<Domain.User.UserRoot.User>;

    internal sealed class GetUserByIdHandler : IHandler<GetUserByIdQuery, Domain.User.UserRoot.User>
    {
        private readonly UserDbContext _context;

        public GetUserByIdHandler(UserDbContext context)
        {
            _context = context;
        }

        public async Task<Domain.User.UserRoot.User> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            var user = await _context.Users.FindAsync(new object[] { request.UserId }, cancellationToken);

            if (user == null)
                throw new InvalidOperationException($"User with ID {request.UserId} not found.");

            return user;
        }
    }
}
