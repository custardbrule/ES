using CQRS;
using Infras.User.Services.Dtos;
using Microsoft.EntityFrameworkCore;
using RequestValidatior;
using Utilities;

namespace Infras.User.Services.Queries
{
    public sealed record GetAllUsersQuery(int Page = 1, int PageSize = 10) : IRequest<PagedList<UserDto>>;

    public sealed class GetAllUsersQueryValidator : BaseValidator<GetAllUsersQuery>
    {
        public GetAllUsersQueryValidator()
        {
            RuleFor(x => x.Page)
                .With(p => p >= 1, "Page must be at least 1.");
            RuleFor(x => x.PageSize)
                .With(s => s >= 10, "PageSize must be at least 10.")
                .With(s => s <= 100, "PageSize must not exceed 100.");
        }
    }

    internal sealed class GetAllUsersHandler(IDbContextFactory<UserDbContext> contextFactory)
        : IHandler<GetAllUsersQuery, PagedList<UserDto>>
    {
        public async Task<PagedList<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            var page = request.Page;
            var pageSize = request.PageSize;

            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

            var totalCount = await context.Users.CountAsync(cancellationToken);

            var users = await context.Users
                .OrderBy(u => u.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserDto(u.Id, u.Account, u.CreatedDate))
                .ToListAsync(cancellationToken);

            return PagedList<UserDto>.Create(users, totalCount, page, pageSize);
        }
    }
}
