using System.Security.Cryptography;
using System.Text;

namespace Infras.User.Services
{
    internal static class CodeGenerator
    {
        private const string Upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string Lower = "abcdefghijklmnopqrstuvwxyz";
        private const string Digits = "0123456789";
        private const string Special = "@$!%*?&";

        public static string GeneratePassword(int length = 12)
        {
            var all = Upper + Lower + Digits + Special;
            var password = new char[length];
            password[0] = Upper[RandomNumberGenerator.GetInt32(Upper.Length)];
            password[1] = Digits[RandomNumberGenerator.GetInt32(Digits.Length)];
            password[2] = Special[RandomNumberGenerator.GetInt32(Special.Length)];
            for (var i = 3; i < length; i++)
                password[i] = all[RandomNumberGenerator.GetInt32(all.Length)];

            RandomNumberGenerator.Shuffle(password.AsSpan());
            return new string(password);
        }

        public static string GenerateRecoveryCode(int length = 8)
        {
            var chars = Upper + Digits;
            var code = new char[length];
            for (var i = 0; i < length; i++)
                code[i] = chars[RandomNumberGenerator.GetInt32(chars.Length)];
            return new string(code);
        }

        public static string HashRecoveryCode(string code)
        {
            var bytes = Encoding.UTF8.GetBytes(code);
            var hash = SHA256.HashData(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
