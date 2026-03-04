using Infras.User.Services;
using Infras.User.Services.Constants;
using Microsoft.AspNetCore.Authentication.Cookies;
using OpenIddict.Abstractions;
using User.Api.Middleware;

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
            var baseUrl = builder.Configuration["BaseUrl"]!;
            builder.Services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("oauth2", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.OAuth2,
                    Flows = new Microsoft.OpenApi.Models.OpenApiOAuthFlows
                    {
                        AuthorizationCode = new Microsoft.OpenApi.Models.OpenApiOAuthFlow
                        {
                            AuthorizationUrl = new Uri($"{baseUrl}{AppOpenIddictConstants.Endpoints.Authorize}"),
                            TokenUrl = new Uri($"{baseUrl}{AppOpenIddictConstants.Endpoints.Token}"),
                            Scopes = new Dictionary<string, string>
                            {
                                { "openid", "OpenID" },
                                { "profile", "Profile" },
                                { "email", "Email" },
                                { "roles", "Roles" },
                                { "offline_access", "Offline Access" }
                            }
                        }
                    }
                });
                options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
                {
                    {
                        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                        {
                            Reference = new Microsoft.OpenApi.Models.OpenApiReference
                            {
                                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                Id = "oauth2"
                            }
                        },
                        ["openid", "profile", "email", "roles", "offline_access"]
                    }
                });
            });

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

            if (builder.Environment.IsDevelopment())
            {
                builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
                    policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
            }

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                await OpenIddictSeeder.SeedAsync(scope.ServiceProvider);
            }

            // Configure the HTTP request pipeline.
            app.UseExceptionHandling();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.OAuthClientId("swagger-client");
                    options.OAuthClientSecret("swagger-secret");
                    options.OAuthUsePkce();
                });
            }
            else
            {
                app.UseHttpsRedirection();
            }

            app.UseStaticFiles();

            app.UseRouting();

            if (app.Environment.IsDevelopment())
            {
                app.UseCors();
            }

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers(); // For API controllers
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}"); // For MVC controllers

            app.Run();
        }

    }
}
