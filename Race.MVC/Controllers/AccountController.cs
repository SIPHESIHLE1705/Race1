using Microsoft.AspNetCore.Mvc;
using Race.MVC.Filters;
using Race.MVC.Helpers;
using Race.MVC.Models;
using Race.MVC.Services;
using Race.MVC.ViewModels;

namespace Race.MVC.Controllers
{
    public class AccountController : BaseController
    {
        private readonly IApiClient _api;

        public AccountController(IApiClient api) => _api = api;

        // ---------- REGISTER ----------
        [HttpGet]
        public IActionResult Register()
        {
            if (HttpContext.Session.IsLoggedIn()) return RedirectToHome();
            return View(new RegisterViewModel());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _api.PostAsync(ApiUrl.Build("api/Users/register",
                ("fullname", model.FullName.Trim()),
                ("email", model.Email.Trim()),
                ("password", model.Password),
                ("role", model.Role)));

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View(model);
            }

            FlashSuccess("Registration successful! Please log in.");
            return RedirectToAction(nameof(Login));
        }

        // ---------- LOGIN ----------
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (HttpContext.Session.IsLoggedIn()) return RedirectToHome();
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            HttpContext.Session.Clear();   // always start from a clean session

            var result = await _api.LoginAsync(model.Email.Trim(), model.Password);
            if (!result.Success || result.Data == null)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                model.Password = string.Empty;
                return View(model);
            }

            HttpContext.Session.SignIn(result.Data);   // userID, role, name

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                return LocalRedirect(model.ReturnUrl);

            return RedirectToHome();
        }

        // ---------- LOGOUT ----------
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _api.LogoutAsync();          // clears the API session
            HttpContext.Session.Clear();       // clears the MVC session
            FlashSuccess("You have been logged out.");
            return RedirectToAction(nameof(Login));
        }

        // ---------- PROFILE (both roles) ----------
        [HttpGet, SessionAuthorize]
        public async Task<IActionResult> Profile()
        {
            var result = await _api.GetAsync<UserDto>("api/Users/me");
            if (result.IsUnauthorized) return SessionExpired();
            if (!result.Success || result.Data == null)
            {
                FlashError(result.Message);
                return RedirectToHome();
            }

            return View(new ProfileViewModel
            {
                FullName = result.Data.FullName,
                EmailAddress = result.Data.EmailAddress,
                UserRole = result.Data.UserRole
            });
        }

        [HttpPost, ValidateAntiForgeryToken, SessionAuthorize]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            model.UserRole = CurrentRole ?? string.Empty;
            if (!ModelState.IsValid) return View(model);

            var result = await _api.PutAsync(ApiUrl.Build("api/Users/me",
                ("fullName", model.FullName.Trim()),
                ("emailAddress", model.EmailAddress.Trim())));

            if (result.IsUnauthorized) return SessionExpired();
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View(model);
            }

            HttpContext.Session.SetString(SessionKeys.FullName, model.FullName.Trim());
            FlashSuccess("Your profile has been updated.");
            return RedirectToAction(nameof(Profile));
        }

        // ---------- ACCESS DENIED ----------
        public IActionResult AccessDenied() => View();

        // Role-based landing page
        private IActionResult RedirectToHome() =>
            CurrentRole == Roles.Organiser
                ? RedirectToAction("Dashboard", "Organiser")
                : RedirectToAction("Index", "Home");
    }
}
