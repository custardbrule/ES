using CQRS;
using Data;
using Domain.User.UserRoot;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Infras.User.Services.Commands
{
    /// <summary>
    /// Generates recovery codes for a user
    /// </summary>
    /// <param name="UserId">The user ID to generate codes for</param>
    /// <param name="Count">Number of codes to generate (default: 10)</param>
    public sealed record GenerateRecoveryCodesCommand(
        Guid UserId,
        int Count = 10
    ) : IRequest<List<string>>;

    internal sealed class GenerateRecoveryCodesHandler(UserDbContext context)
        : IHandler<GenerateRecoveryCodesCommand, List<string>>
    {
        public async Task<List<string>> Handle(GenerateRecoveryCodesCommand request, CancellationToken cancellationToken)
        {
            var user = await context.Users
                .Include(u => u.RecoveryCodes)
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken)
                ?? throw new InvalidOperationException("User not found");

            // Remove all existing unused recovery codes
            var existingCodes = user.RecoveryCodes.Where(rc => !rc.IsUsed).ToList();
            foreach (var code in existingCodes)
            {
                context.RecoveryCodes.Remove(code);
            }

            // Generate new recovery codes
            var plainTextCodes = new List<string>();
            var recoveryCodes = new List<RecoveryCode>();

            for (int i = 0; i < request.Count; i++)
            {
                // Generate a random recovery code (8 characters, alphanumeric)
                var plainCode = GenerateRandomCode(8);
                plainTextCodes.Add(plainCode);

                // Hash the code before storing
                var hashedCode = HashCode(plainCode);
                var recoveryCode = RecoveryCode.Create(request.UserId, hashedCode);
                recoveryCodes.Add(recoveryCode);
            }

            // Add new recovery codes to database
            await context.RecoveryCodes.AddRangeAsync(recoveryCodes, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            return plainTextCodes;
        }

        private static string GenerateRandomCode(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var code = new char[length];

            for (int i = 0; i < length; i++)
            {
                code[i] = chars[RandomNumberGenerator.GetInt32(chars.Length)];
            }

            return new string(code);
        }

        private static string HashCode(string code)
        {
            var bytes = Encoding.UTF8.GetBytes(code);
            var hash = SHA256.HashData(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
