using Seed;

namespace Domain.User.UserRoot
{
    public record Role(string Name, string Description) : BaseAuditEntity<Guid>(Guid.NewGuid(), DateTimeOffset.UtcNow)
    {
        private Role() : this("", "") { }

        // Navigation property for role scopes
        public ICollection<RoleScope> RoleScopes { get; init; } = [];
    }
}
