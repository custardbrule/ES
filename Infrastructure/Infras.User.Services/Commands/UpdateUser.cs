using System.Text.RegularExpressions;
using CQRS;
using Microsoft.EntityFrameworkCore;
using RequestValidatior;
using Seed;
using Utilities;

namespace Infras.User.Services.Commands
{
    public sealed record UpdateUserCommand(Guid UserId, string Account) : IRequest<Unit>;

    public sealed partial class UpdateUserCommandValidator : BaseValidator<UpdateUserCommand>
    {
        [GeneratedRegex(ValidationConstants.AccountPattern)]
        private static partial Regex AccountRegex();

        public UpdateUserCommandValidator()
        {
            RuleFor(x => x.Account)
                .With(account => !string.IsNullOrWhiteSpace(account), "Account is required")
                .With(account => AccountRegex().IsMatch(account), ValidationConstants.AccountValidationMessage);
        }
    }

    internal sealed class UpdateUserHandler(IDbContextFactory<UserDbContext> contextFactory)
        : IHandler<UpdateUserCommand, Unit>
    {
        public async Task<Unit> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

            var user = await context.Users.FindAsync(new object[] { request.UserId }, cancellationToken)
                ?? throw new BussinessException("USER_NOT_FOUND", 404, $"User with ID {request.UserId} not found.");

            var duplicate = await context.Users
                .AnyAsync(u => u.Account == request.Account && u.Id != request.UserId, cancellationToken);

            if (duplicate)
                throw new BussinessException("DUPLICATE_ACCOUNT", 409, "Account already exists.");

            await context.Users
                .Where(u => u.Id == request.UserId)
                .ExecuteUpdateAsync(s => s.SetProperty(u => u.Account, request.Account), cancellationToken);

            return Unit.Value;
        }
    }
}
