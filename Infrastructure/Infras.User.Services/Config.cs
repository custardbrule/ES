using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CQRS;
using App.Extensions.DependencyInjection;
using System.Reflection;
using Infras.User.Services.Pipelines;
using Infras.User.Services.Constants;

namespace Infras.User.Services
{
    public static class Config
    {
        public static IServiceCollection ConfigInfras(this IServiceCollection services, IConfiguration configuration)
        {
            services.RegisterDbContextPool<UserDbContext>(configuration.GetConnectionString("UserConnection")!);
            services.AddElasticsearchCore(configuration);
            services.AddCQRS(ServiceLifetime.Transient, Assembly.GetExecutingAssembly());
            services.AddScoped(typeof(IPipeline<,>), typeof(LogPipe<,>));
            services.RegisterQuartz(configuration);
            services.RegisterKafkaServices(configuration, Assembly.GetExecutingAssembly());

            // Configure OpenIddict
            services.AddOpenIddict()
                .AddCore(options =>
                {
                    options.UseEntityFrameworkCore()
                        .UseDbContext<UserDbContext>();

                    // Use Quartz.NET for token cleanup (already registered above)
                    options.UseQuartz();
                })
                .AddServer(options =>
                {
                    // Enable the authorization, token and userinfo endpoints
                    options.SetAuthorizationEndpointUris(OpenIddictConstants.Endpoints.Authorize)
                        .SetTokenEndpointUris(OpenIddictConstants.Endpoints.Token)
                        .SetUserinfoEndpointUris(OpenIddictConstants.Endpoints.Userinfo)
                        .SetLogoutEndpointUris(OpenIddictConstants.Endpoints.Logout);

                    // Enable the authorization code flow with PKCE
                    options.AllowAuthorizationCodeFlow()
                        .RequireProofKeyForCodeExchange()
                        .AllowRefreshTokenFlow()
                        .AllowClientCredentialsFlow();

                    // Configure token formats - Use JWT for access tokens
                    options.UseAccessTokens()
                        .UseJsonWebTokens();

                    // Register signing and encryption credentials
                    options.AddDevelopmentEncryptionCertificate()
                        .AddDevelopmentSigningCertificate();

                    // Configure token lifetimes
                    options.SetAccessTokenLifetime(TimeSpan.FromMinutes(30))
                        .SetRefreshTokenLifetime(TimeSpan.FromDays(7))
                        .SetAuthorizationCodeLifetime(TimeSpan.FromMinutes(5));

                    // Register ASP.NET Core host and configure ASP.NET Core-specific options
                    options.UseAspNetCore()
                        .EnableAuthorizationEndpointPassthrough()
                        .EnableTokenEndpointPassthrough()
                        .EnableUserinfoEndpointPassthrough()
                        .EnableLogoutEndpointPassthrough();
                })
                .AddValidation(options =>
                {
                    options.UseLocalServer();
                    options.UseAspNetCore();
                });

            return services;
        }
    }
}
