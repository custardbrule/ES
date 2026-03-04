using Infras.User.Services;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace User.Api
{
    public static class OpenIddictSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var manager = serviceProvider.GetRequiredService<IOpenIddictApplicationManager>();
            var scopeManager = serviceProvider.GetRequiredService<IOpenIddictScopeManager>();
            var context = serviceProvider.GetRequiredService<UserDbContext>();

            await context.Database.MigrateAsync();

            if (await scopeManager.FindByNameAsync(Scopes.OpenId) == null)
            {
                await scopeManager.CreateAsync(new OpenIddictScopeDescriptor { Name = Scopes.OpenId, DisplayName = "OpenID" });
                await scopeManager.CreateAsync(new OpenIddictScopeDescriptor { Name = Scopes.Email, DisplayName = "Email" });
                await scopeManager.CreateAsync(new OpenIddictScopeDescriptor { Name = Scopes.Profile, DisplayName = "Profile" });
                await scopeManager.CreateAsync(new OpenIddictScopeDescriptor { Name = Scopes.Roles, DisplayName = "Roles" });
                await scopeManager.CreateAsync(new OpenIddictScopeDescriptor { Name = Scopes.OfflineAccess, DisplayName = "Offline Access" });
            }

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
                        $"{Permissions.Prefixes.Scope}{Scopes.OfflineAccess}",
                        Requirements.Features.ProofKeyForCodeExchange
                    }
                });
            }

            if (await manager.FindByClientIdAsync("diary-client") == null)
            {
                await manager.CreateAsync(new OpenIddictApplicationDescriptor
                {
                    ClientId = "diary-client",
                    ClientType = ClientTypes.Public,
                    DisplayName = "Diary Client Application",
                    RedirectUris = { new Uri("https://localhost:7001/signin-oidc"), new Uri("http://localhost:4200/callback") },
                    PostLogoutRedirectUris = { new Uri("https://localhost:7001/signout-callback-oidc"), new Uri("https://localhost:4200/signout-callback-oidc") },
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
                        $"{Permissions.Prefixes.Scope}{Scopes.OfflineAccess}",
                        Requirements.Features.ProofKeyForCodeExchange
                    }
                });
            }

            if (await manager.FindByClientIdAsync("diary-swagger-client") == null)
            {
                await manager.CreateAsync(new OpenIddictApplicationDescriptor
                {
                    ClientId = "diary-swagger-client",
                    ClientType = ClientTypes.Public,
                    DisplayName = "Diary API Swagger Client",
                    RedirectUris = { new Uri("http://localhost:5075/swagger/oauth2-redirect.html") },
                    Permissions =
                    {
                        Permissions.Endpoints.Authorization,
                        Permissions.Endpoints.Token,
                        Permissions.GrantTypes.AuthorizationCode,
                        Permissions.ResponseTypes.Code,
                        $"{Permissions.Prefixes.Scope}{Scopes.OpenId}",
                        Permissions.Scopes.Email,
                        Permissions.Scopes.Profile,
                        Permissions.Scopes.Roles,
                        $"{Permissions.Prefixes.Scope}{Scopes.OfflineAccess}",
                        Requirements.Features.ProofKeyForCodeExchange
                    }
                });
            }

            if (await manager.FindByClientIdAsync("swagger-client") == null)
            {
                await manager.CreateAsync(new OpenIddictApplicationDescriptor
                {
                    ClientId = "swagger-client",
                    ClientSecret = "swagger-secret",
                    DisplayName = "Swagger API Client",
                    RedirectUris = { new Uri("http://localhost:5100/swagger/oauth2-redirect.html") },
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
