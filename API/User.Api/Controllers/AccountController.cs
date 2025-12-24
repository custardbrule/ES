using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using User.Api.Models;
using CQRS;
using Data;
using Infras.User.Services.Commands;
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
    }
}
