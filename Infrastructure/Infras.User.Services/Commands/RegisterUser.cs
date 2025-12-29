using CQRS;
using Data;
using Domain.User.UserRoot;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Infras.User.Services.Commands
{
    /// <summary>
    /// Registers a new user and generates recovery codes
    /// </summary>
    /// <param name="Account">User account name</param>
    /// <param name="Password">User password</param>
    public sealed record RegisterUserCommand(
        string Account,
        string Password
    ) : IRequest<RegisterUserResult>;

    public sealed record RegisterUserResult(
        Guid UserId,
        List<string> RecoveryCodes
    );

    internal sealed class RegisterUserHandler(UserDbContext context, IConfiguration configuration)
        : IHandler<RegisterUserCommand, RegisterUserResult>
    {
        public async Task<RegisterUserResult> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            // Check if account already exists
            var existingUser = await context.Users
                .FirstOrDefaultAsync(u => u.Account == request.Account, cancellationToken);

            if (existingUser != null)
            {
                throw new InvalidOperationException("Account already exists");
            }

            // Get secret key from configuration
            var secretKey = configuration["Authentication:SecretKey"] ?? "default-secret-key";

            // Create new user
            var user = Domain.User.UserRoot.User.Create(request.Account, request.Password, secretKey);
            await context.Users.AddAsync(user, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            // Generate recovery codes
            var plainTextCodes = new List<string>();
            var recoveryCodes = new List<RecoveryCode>();

            for (int i = 0; i < 10; i++)
            {
                var plainCode = GenerateRandomCode(8);
                plainTextCodes.Add(plainCode);

                var hashedCode = HashCode(plainCode);
                var recoveryCode = RecoveryCode.Create(user.Id, hashedCode);
                recoveryCodes.Add(recoveryCode);
            }

            await context.RecoveryCodes.AddRangeAsync(recoveryCodes, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            return new RegisterUserResult(user.Id, plainTextCodes);
        }

        private static string GenerateRandomCode(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var code = new char[length];

            for (int i = 0; i < length; i++)
            {
                code[i] = chars[System.Security.Cryptography.RandomNumberGenerator.GetInt32(chars.Length)];
            }

            return new string(code);
        }

        private static string HashCode(string code)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(code);
            var hash = System.Security.Cryptography.SHA256.HashData(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
