using Infras.User.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using User.Api.Middleware;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace User.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Configure infrastructure services (includes OpenIddict)
            builder.Services.ConfigInfras(builder.Configuration);

            // Configure cookie authentication for login UI
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.LogoutPath = "/Account/Logout";
                });

            builder.Services.AddAuthorization();

            var app = builder.Build();

            // Seed OpenIddict clients
            using (var scope = app.Services.CreateScope())
            {
                await SeedOpenIddictData(scope.ServiceProvider);
            }

            // Configure the HTTP request pipeline.
            app.UseExceptionHandling();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers(); // For API controllers
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}"); // For MVC controllers

            app.Run();
        }

        private static async Task SeedOpenIddictData(IServiceProvider serviceProvider)
        {
            var manager = serviceProvider.GetRequiredService<IOpenIddictApplicationManager>();
            var scopeManager = serviceProvider.GetRequiredService<IOpenIddictScopeManager>();
            var context = serviceProvider.GetRequiredService<UserDbContext>();

            // Apply pending migrations
            await context.Database.MigrateAsync();

            // Seed scopes if not exist
            if (await scopeManager.FindByNameAsync(Scopes.OpenId) == null)
            {
                await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
                {
                    Name = Scopes.OpenId,
                    DisplayName = "OpenID"
                });

                await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
                {
                    Name = Scopes.Email,
                    DisplayName = "Email"
                });

                await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
                {
                    Name = Scopes.Profile,
                    DisplayName = "Profile"
                });

                await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
                {
                    Name = Scopes.Roles,
                    DisplayName = "Roles"
                });
            }

            // Check if client already exists
            if (await manager.FindByClientIdAsync("ums-client") == null)
            {
                await manager.CreateAsync(new OpenIddictApplicationDescriptor
                {
                    ClientId = "ums-client",
                    ClientSecret = "ums-client-secret",
                    DisplayName = "UMS Client Application",
                    RedirectUris = { new Uri("https://localhost:7001/signin-oidc"), new Uri("http://localhost:5173/callback") },
                    PostLogoutRedirectUris = { new Uri("https://localhost:7001/signout-callback-oidc"), new Uri("https://localhost:5173/signout-callback-oidc") },
                    Permissions =
                    {
                        Permissions.Endpoints.Authorization,
                        Permissions.Endpoints.Token,
                        Permissions.Endpoints.Logout,
                        Permissions.GrantTypes.AuthorizationCode,
                        Permissions.GrantTypes.RefreshToken,
                        Permissions.ResponseTypes.Code,
                        $"{Permissions.Prefixes.Scope}{Scopes.OpenId}",
                        Permissions.Scopes.Email,
                        Permissions.Scopes.Profile,
                        Permissions.Scopes.Roles,
                        Requirements.Features.ProofKeyForCodeExchange
                    }
                });
            }

            // Add a Swagger/API client
            if (await manager.FindByClientIdAsync("swagger-client") == null)
            {
                await manager.CreateAsync(new OpenIddictApplicationDescriptor
                {
                    ClientId = "swagger-client",
                    ClientSecret = "swagger-secret",
                    DisplayName = "Swagger API Client",
                    RedirectUris = { new Uri("https://localhost:7000/swagger/oauth2-redirect.html") },
                    Permissions =
                    {
                        Permissions.Endpoints.Authorization,
                        Permissions.Endpoints.Token,
                        Permissions.GrantTypes.AuthorizationCode,
                        Permissions.GrantTypes.ClientCredentials,
                        Permissions.ResponseTypes.Code,
                        $"{Permissions.Prefixes.Scope}{Scopes.OpenId}",
                        Permissions.Scopes.Email,
                        Permissions.Scopes.Profile,
                        Requirements.Features.ProofKeyForCodeExchange
                    }
                });
            }
        }
    }
}
