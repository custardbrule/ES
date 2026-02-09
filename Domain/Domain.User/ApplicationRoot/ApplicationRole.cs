namespace Domain.User.ApplicationRoot
{
    public record ApplicationRole(Guid Id, string ApplicationId, string Name, string Description)
    {
        private ApplicationRole() : this(Guid.NewGuid(), "", "", "") { }

        public ICollection<ApplicationRoleScope> ApplicationRoleScopes { get; init; } = [];
    }
}
