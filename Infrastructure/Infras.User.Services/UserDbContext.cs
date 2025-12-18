using Microsoft.EntityFrameworkCore;
using Domain.User.UserRoot;

namespace Infras.User.Services
{
    public class UserDbContext : DbContext
    {
        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
        {
        }

        public DbSet<Domain.User.UserRoot.User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<UserScope> UserScopes { get; set; }
        public DbSet<RoleScope> RoleScopes { get; set; }
        public DbSet<LoginHistory> LoginHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure OpenIddict entities
            modelBuilder.UseOpenIddict();
        }
    }
}
