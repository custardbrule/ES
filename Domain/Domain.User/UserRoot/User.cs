using Seed;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;

namespace Domain.User.UserRoot
{
    public sealed record User : BaseAuditEntity<Guid>
    {
        public string Account { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;

        [JsonConstructor]
        private User() : base(Guid.NewGuid(), DateTimeOffset.UtcNow) { }

        private User(string account, string password, string secretKey) : base(Guid.NewGuid(), DateTimeOffset.UtcNow)
        {
            Account = account;
            Password = HashPassword(password, secretKey);
        }

        // Navigation properties for EF Core relationships
        public ICollection<UserRole> UserRoles { get; init; } = [];
        public ICollection<LoginHistory> LoginHistories { get; init; } = [];
        public ICollection<UserScope> Scopes { get; init; } = [];
        public ICollection<RecoveryCode> RecoveryCodes { get; init; } = [];

        /// <summary>
        /// Creates a new user with the specified account and hashed password
        /// </summary>
        /// <param name="account">User account name</param>
        /// <param name="password">Plain text password to be hashed</param>
        /// <param name="secretKey">Secret key for password hashing</param>
        /// <returns>A new User instance</returns>
        public static User Create(string account, string password, string secretKey)
            => new User(account.Trim(), password, secretKey);

        /// <summary>
        /// Validates if the provided password matches the stored password hash
        /// </summary>
        /// <param name="password">Plain text password to validate</param>
        /// <param name="secretKey">Secret key for password hashing</param>
        /// <returns>True if password is valid, otherwise false</returns>
        public bool ValidatePassword(string password, string secretKey)
        {
            if (string.IsNullOrWhiteSpace(password)) return false;
            return Password == HashPassword(password, secretKey);
        }

        /// <summary>
        /// Changes the user password after validating the current password
        /// </summary>
        /// <param name="currentPassword">Current password for validation</param>
        /// <param name="newPassword">New password to set</param>
        /// <param name="secretKey">Secret key for password hashing</param>
        /// <returns>A new User instance with updated password</returns>
        /// <exception cref="InvalidOperationException">Thrown when current password is incorrect</exception>
        public User ChangePassword(string currentPassword, string newPassword, string secretKey)
        {
            if (!ValidatePassword(currentPassword, secretKey)) throw new InvalidOperationException("Current password is incorrect");
            return this with { Password = HashPassword(newPassword, secretKey) };
        }

        /// <summary>
        /// Adds a role to the user
        /// </summary>
        /// <param name="roleId">The ID of the role to add</param>
        /// <exception cref="ArgumentException">Thrown when roleId is empty</exception>
        public void AddUserRole(Guid roleId)
        {
            if (roleId == Guid.Empty) throw new ArgumentException("RoleId cannot be empty", nameof(roleId));

            var userRole = new UserRole(Guid.NewGuid(), Id, roleId);
            UserRoles.Add(userRole); // Add to collection so EF Core tracks it
        }

        /// <summary>
        /// Removes a role from the user by role ID
        /// </summary>
        /// <param name="roleId">The ID of the role to remove</param>
        /// <returns>True if the role was removed, otherwise false</returns>
        public bool RemoveUserRole(Guid roleId)
        {
            if (roleId == Guid.Empty) return false;

            var userRole = UserRoles.FirstOrDefault(ur => ur.RoleId == roleId);
            if (userRole == null) return false;

            return UserRoles.Remove(userRole); // EF Core tracks removal
        }

        /// <summary>
        /// Removes a role from the user by UserRole entity
        /// </summary>
        /// <param name="userRole">The UserRole entity to remove</param>
        /// <returns>True if the role was removed, otherwise false</returns>
        /// <exception cref="ArgumentNullException">Thrown when userRole is null</exception>
        public bool RemoveUserRole(UserRole userRole)
        {
            ArgumentNullException.ThrowIfNull(userRole);
            return UserRoles.Remove(userRole);
        }

        /// <summary>
        /// Adds a scope to the user
        /// </summary>
        /// <param name="scope">The scope value to add</param>
        /// <exception cref="ArgumentException">Thrown when scope is empty or whitespace</exception>
        public void AddUserScope(string scope)
        {
            if (string.IsNullOrWhiteSpace(scope)) throw new ArgumentException("Scope cannot be empty", nameof(scope));

            var userScope = new UserScope(Guid.NewGuid(), Id, scope);
            Scopes.Add(userScope); // Add to collection so EF Core tracks it
        }

        /// <summary>
        /// Removes a scope from the user by scope value
        /// </summary>
        /// <param name="scope">The scope value to remove</param>
        /// <returns>True if the scope was removed, otherwise false</returns>
        public bool RemoveUserScope(string scope)
        {
            if (string.IsNullOrWhiteSpace(scope)) return false;

            var userScope = Scopes.FirstOrDefault(s => s.Scope == scope);
            if (userScope == null) return false;

            return Scopes.Remove(userScope); // EF Core tracks removal
        }

        /// <summary>
        /// Removes a scope from the user by UserScope entity
        /// </summary>
        /// <param name="scope">The UserScope entity to remove</param>
        /// <returns>True if the scope was removed, otherwise false</returns>
        /// <exception cref="ArgumentNullException">Thrown when scope is null</exception>
        public bool RemoveUserScope(UserScope scope)
        {
            ArgumentNullException.ThrowIfNull(scope);
            return Scopes.Remove(scope);
        }

        /// <summary>
        /// Gets the claims for this user including roles and scopes
        /// </summary>
        /// <returns>A list of claims for authentication</returns>
        public List<Claim> GetClaims()
        {
            return
            [
                new(ClaimTypes.NameIdentifier, Id.ToString()),
                new(ClaimTypes.Name, Account),
                ..UserRoles.Select(ur => new Claim(ClaimTypes.Role, ur.RoleId.ToString())),
                ..Scopes.Select(s => new Claim("scope", s.Scope))
            ];
        }

        #region Private methods
        // Hash password using HMACSHA256 with secret key
        private string HashPassword(string password, string secretKey)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secretKey);
            var passwordBytes = Encoding.UTF8.GetBytes(password);
            var hash = HMACSHA256.HashData(keyBytes, passwordBytes);
            return Convert.ToBase64String(hash);
        }
        #endregion
    }
}
