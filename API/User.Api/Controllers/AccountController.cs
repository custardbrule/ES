using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Infras.User.Services;
using User.Api.Models;

namespace User.Api.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserDbContext _context;
        private readonly IConfiguration _configuration;

        public AccountController(UserDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string account, string password, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (string.IsNullOrWhiteSpace(account) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError(string.Empty, "Account and password are required.");
                return View();
            }

            // Find user by account
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Account == account);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid account or password.");
                return View();
            }

            // Validate password using the User domain method
            var secretKey = _configuration["Authentication:SecretKey"] ?? "default-secret-key";
            if (!user.ValidatePassword(password, secretKey))
            {
                ModelState.AddModelError(string.Empty, "Invalid account or password.");
                return View();
            }

            // Create claims principal
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Account)
            };

            var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            // Sign in the user
            await HttpContext.SignInAsync("Cookies", claimsPrincipal);

            // Log login history
            var loginHistory = new Domain.User.UserRoot.LoginHistory(
                Guid.NewGuid(),
                user.Id,
                DateTimeOffset.UtcNow,
                HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                HttpContext.Request.Headers.UserAgent.ToString()
            );
            _context.LoginHistories.Add(loginHistory);
            await _context.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("Cookies");
            return RedirectToAction("Index", "Home");
        }
    }
}
