using CQRS;
using Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Infras.User.Services.Commands
{
    /// <summary>
    /// Validates a recovery code and marks it as used
    /// </summary>
    /// <param name="Account">User account</param>
    /// <param name="RecoveryCode">The recovery code to validate</param>
    public sealed record ValidateRecoveryCodeCommand(
        string Account,
        string RecoveryCode
    ) : IRequest<Guid?>;

    internal sealed class ValidateRecoveryCodeHandler(UserDbContext context)
        : IHandler<ValidateRecoveryCodeCommand, Guid?>
    {
        public async Task<Guid?> Handle(ValidateRecoveryCodeCommand request, CancellationToken cancellationToken)
        {
            var user = await context.Users
                .Include(u => u.RecoveryCodes)
                .FirstOrDefaultAsync(u => u.Account == request.Account, cancellationToken);

            if (user == null) return null;

            // Hash the provided recovery code
            var hashedCode = HashCode(request.RecoveryCode);

            // Find matching unused recovery code
            var recoveryCode = user.RecoveryCodes
                .FirstOrDefault(rc => !rc.IsUsed && rc.CodeHash == hashedCode);

            if (recoveryCode == null) return null;

            // Mark as used
            var updatedCode = recoveryCode.MarkAsUsed();
            context.Entry(recoveryCode).CurrentValues.SetValues(updatedCode);
            await context.SaveChangesAsync(cancellationToken);

            return user.Id;
        }

        private static string HashCode(string code)
        {
            var bytes = Encoding.UTF8.GetBytes(code);
            var hash = SHA256.HashData(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
