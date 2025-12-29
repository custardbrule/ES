using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using User.Api.Models;
using CQRS;
using Data;
using Infras.User.Services.Commands;
using Infras.User.Services.Queries;
using Infras.User.Services.Jobs;
using Quartz;

namespace User.Api.Controllers
{
    public class AccountController : Controller
    {
        private readonly IPublisher _publisher;
        private readonly IQuartzJobManager _jobManager;

        public AccountController(IPublisher publisher, IQuartzJobManager jobManager)
        {
            _publisher = publisher;
            _jobManager = jobManager;
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

            // Authenticate user
            var user = await _publisher.Send<AuthenticateUserCommand, Domain.User.UserRoot.User?>(
                new AuthenticateUserCommand(account, password));

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid account or password.");
                return View();
            }

            // Create claims principal
            var claimsIdentity = new ClaimsIdentity(user.GetClaims(), CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            // Sign in the user
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

            // Log login history in background
            var jobData = new JobDataMap
            {
                { LogLoginHistoryJob.UserId, user.Id.ToString() },
                { LogLoginHistoryJob.IpAddress, HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown" },
                { LogLoginHistoryJob.UserAgent, HttpContext.Request.Headers.UserAgent.ToString() }
            };
            await _jobManager.Fire<LogLoginHistoryJob>(jobData);

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string account, string password, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(account) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError(string.Empty, "Account and password are required.");
                return View();
            }

            if (account.Length < 3)
            {
                ModelState.AddModelError(string.Empty, "Account must be at least 3 characters.");
                return View();
            }

            if (password.Length < 6)
            {
                ModelState.AddModelError(string.Empty, "Password must be at least 6 characters.");
                return View();
            }

            if (password != confirmPassword)
            {
                ModelState.AddModelError(string.Empty, "Passwords do not match.");
                return View();
            }

            try
            {
                var result = await _publisher.Send<RegisterUserCommand, RegisterUserResult>(
                    new RegisterUserCommand(account, password));

                // Sign in the user
                var user = await _publisher.Send<GetUserByIdQuery, Domain.User.UserRoot.User>(
                    new GetUserByIdQuery(result.UserId));

                var claimsIdentity = new ClaimsIdentity(user.GetClaims(), CookieAuthenticationDefaults.AuthenticationScheme);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

                // Show recovery codes
                TempData["RecoveryCodes"] = System.Text.Json.JsonSerializer.Serialize(result.RecoveryCodes);
                return RedirectToAction("ShowRecoveryCodes");
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View();
            }
        }

        [HttpGet]
        public IActionResult ShowRecoveryCodes()
        {
            if (TempData["RecoveryCodes"] is string json)
            {
                var codes = System.Text.Json.JsonSerializer.Deserialize<List<string>>(json);
                return View("GenerateRecoveryCodes", codes);
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> GenerateRecoveryCodes()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login");
            }

            var codes = await _publisher.Send<GenerateRecoveryCodesCommand, List<string>>(
                new GenerateRecoveryCodesCommand(Guid.Parse(userId)));

            return View(codes);
        }

        [HttpPost]
        public IActionResult ConfirmRecoveryCodes()
        {
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult RecoverAccount()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RecoverAccount(string account, string recoveryCode)
        {
            if (string.IsNullOrWhiteSpace(account) || string.IsNullOrWhiteSpace(recoveryCode))
            {
                ModelState.AddModelError(string.Empty, "Account and recovery code are required.");
                return View();
            }

            var userId = await _publisher.Send<ValidateRecoveryCodeCommand, Guid?>(
                new ValidateRecoveryCodeCommand(account, recoveryCode));

            if (userId == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid account or recovery code.");
                return View();
            }

            // Get user and sign in
            var user = await _publisher.Send<GetUserByIdQuery, Domain.User.UserRoot.User>(
                new GetUserByIdQuery(userId.Value));

            var claimsIdentity = new ClaimsIdentity(user.GetClaims(), CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

            // Redirect to generate new recovery codes
            TempData["Message"] = "Account recovered successfully! Please generate new recovery codes.";
            return RedirectToAction("GenerateRecoveryCodes");
        }
    }
}
