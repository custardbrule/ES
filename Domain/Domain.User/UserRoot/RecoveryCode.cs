using Seed;

namespace Domain.User.UserRoot
{
    /// <summary>
    /// Represents a hashed recovery code for account recovery
    /// Each code can only be used once
    /// </summary>
    public sealed record RecoveryCode : BaseAuditEntity<Guid>
    {
        public Guid UserId { get; init; }
        public string CodeHash { get; init; } = string.Empty;
        public bool IsUsed { get; private set; }
        public DateTimeOffset? UsedAt { get; private set; }

        private RecoveryCode() : base(Guid.NewGuid(), DateTimeOffset.UtcNow) { }

        private RecoveryCode(Guid userId, string codeHash) : base(Guid.NewGuid(), DateTimeOffset.UtcNow)
        {
            UserId = userId;
            CodeHash = codeHash;
            IsUsed = false;
        }

        public static RecoveryCode Create(Guid userId, string codeHash)
            => new RecoveryCode(userId, codeHash);

        /// <summary>
        /// Marks the recovery code as used
        /// </summary>
        public RecoveryCode MarkAsUsed()
        {
            return this with
            {
                IsUsed = true,
                UsedAt = DateTimeOffset.UtcNow
            };
        }
    }
}
