using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using System.Security.Claims;
using Infras.User.Services;
using Infras.User.Services.Constants;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace User.Api.Controllers
{
    public class AuthController : Controller
    {
        private readonly UserDbContext _context;
        private readonly IOpenIddictApplicationManager _applicationManager;
        private readonly IOpenIddictAuthorizationManager _authorizationManager;
        private readonly IOpenIddictScopeManager _scopeManager;

        public AuthController(
            UserDbContext context,
            IOpenIddictApplicationManager applicationManager,
            IOpenIddictAuthorizationManager authorizationManager,
            IOpenIddictScopeManager scopeManager)
        {
            _context = context;
            _applicationManager = applicationManager;
            _authorizationManager = authorizationManager;
            _scopeManager = scopeManager;
        }

        [HttpGet($"~{OpenIddictConstants.Endpoints.Authorize}")]
        [HttpPost($"~{OpenIddictConstants.Endpoints.Authorize}")]
        public async Task<IActionResult> Authorize()
        {
            var request = HttpContext.GetOpenIddictServerRequest() ??
                throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

            // Retrieve the user principal stored in the authentication cookie
            var result = await HttpContext.AuthenticateAsync();

            // If the user is not authenticated, redirect to the login page
            if (result == null || !result.Succeeded || request.HasPrompt(Prompts.Login))
            {
                return Challenge(
                    authenticationSchemes: "Cookies",
                    properties: new AuthenticationProperties
                    {
                        RedirectUri = Request.PathBase + Request.Path + QueryString.Create(
                            Request.HasFormContentType ? Request.Form.ToList() : Request.Query.ToList())
                    });
            }

            // Retrieve the user from database
            var userId = result.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _context.Users.FindAsync(Guid.Parse(userId!));

            if (user == null)
            {
                return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            // Create the claims-based identity
            var identity = new ClaimsIdentity(
                authenticationType: TokenValidationParameters.DefaultAuthenticationType,
                nameType: Claims.Name,
                roleType: Claims.Role);

            identity.AddClaim(Claims.Subject, user.Id.ToString());
            identity.AddClaim(Claims.Name, user.Account);

            // Add user scopes as claims
            var userScopes = await _context.UserScopes
                .Where(us => us.UserId == user.Id)
                .Select(us => us.Scope)
                .ToListAsync();

            foreach (var scope in userScopes)
            {
                identity.AddClaim(Claims.Role, scope);
            }

            var principal = new ClaimsPrincipal(identity);

            // Set the list of scopes granted to the client application
            principal.SetScopes(request.GetScopes());

            // Automatically create a permanent authorization to avoid requiring explicit consent
            // for future authorization or token requests containing the same scopes
            var authorization = await _authorizationManager.FindBySubjectAsync(user.Id.ToString());
            if (authorization == null)
            {
                authorization = await _authorizationManager.CreateAsync(
                    principal: principal,
                    subject: user.Id.ToString(),
                    client: request.ClientId!,
                    type: AuthorizationTypes.Permanent,
                    scopes: principal.GetScopes());
            }

            principal.SetAuthorizationId(await _authorizationManager.GetIdAsync(authorization));

            // Set the resource servers the access token should be issued for
            principal.SetResources(await _scopeManager.ListResourcesAsync(principal.GetScopes()).ToListAsync());

            // Sign in the user
            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        [HttpPost($"~{OpenIddictConstants.Endpoints.Token}")]
        public async Task<IActionResult> Token()
        {
            var request = HttpContext.GetOpenIddictServerRequest() ??
                throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

            if (request.IsAuthorizationCodeGrantType() || request.IsRefreshTokenGrantType())
            {
                // Retrieve the claims principal stored in the authorization code/refresh token
                var principal = (await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)).Principal;

                // Retrieve the user from database
                var userId = principal?.GetClaim(Claims.Subject);
                var user = await _context.Users.FindAsync(Guid.Parse(userId!));

                if (user == null)
                {
                    return Forbid(
                        authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                        properties: new AuthenticationProperties(new Dictionary<string, string?>
                        {
                            [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                            [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The token is no longer valid."
                        }));
                }

                // Ensure the user is still allowed to sign in
                var identity = new ClaimsIdentity(principal!.Claims,
                    authenticationType: TokenValidationParameters.DefaultAuthenticationType,
                    nameType: Claims.Name,
                    roleType: Claims.Role);

                // Override the user claims present in the principal if they changed since the authorization code was issued
                identity.SetClaim(Claims.Subject, user.Id.ToString())
                        .SetClaim(Claims.Name, user.Account);

                principal = new ClaimsPrincipal(identity);

                principal.SetScopes(request.GetScopes());
                principal.SetResources(await _scopeManager.ListResourcesAsync(principal.GetScopes()).ToListAsync());

                return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }
            else if (request.IsClientCredentialsGrantType())
            {
                // Note: the client credentials flow is mainly used by machine-to-machine communication
                var identity = new ClaimsIdentity(
                    authenticationType: TokenValidationParameters.DefaultAuthenticationType,
                    nameType: Claims.Name,
                    roleType: Claims.Role);

                identity.SetClaim(Claims.Subject, request.ClientId ?? throw new InvalidOperationException());

                var principal = new ClaimsPrincipal(identity);

                principal.SetScopes(request.GetScopes());
                principal.SetResources(await _scopeManager.ListResourcesAsync(principal.GetScopes()).ToListAsync());

                return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            throw new InvalidOperationException("The specified grant type is not supported.");
        }

        [HttpGet($"~{OpenIddictConstants.Endpoints.Userinfo}")]
        [HttpPost($"~{OpenIddictConstants.Endpoints.Userinfo}")]
        public async Task<IActionResult> Userinfo()
        {
            var principal = (await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)).Principal;

            var userId = principal?.GetClaim(Claims.Subject);
            var user = await _context.Users
                .Include(u => u.Scopes)
                .FirstOrDefaultAsync(u => u.Id == Guid.Parse(userId!));

            if (user == null)
            {
                return Challenge(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidToken,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The specified access token is not valid."
                    }));
            }

            var claims = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                [Claims.Subject] = user.Id.ToString(),
                [Claims.Name] = user.Account
            };

            if (principal!.HasScope(Scopes.Email))
            {
                claims[Claims.Email] = user.Account;
            }

            if (principal.HasScope(Scopes.Roles))
            {
                claims[Claims.Role] = user.Scopes.Select(s => s.Scope).ToArray();
            }

            return Ok(claims);
        }

        [HttpGet($"~{OpenIddictConstants.Endpoints.Logout}")]
        [HttpPost($"~{OpenIddictConstants.Endpoints.Logout}")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("Cookies");

            return SignOut(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties
                {
                    RedirectUri = "/"
                });
        }
    }
}
