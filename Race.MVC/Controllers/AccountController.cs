
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
        // API client used to communicate with the API.
        private readonly IApiClient _api;

        // Constructor receives the API client through dependency injection.
        public AccountController(IApiClient api) => _api = api;

        // ---------- REGISTER ----------

        // Displays the registration page.
        [HttpGet]
        public IActionResult Register()
        {
            // Redirects logged-in users away from the registration page.
            if (HttpContext.Session.IsLoggedIn()) return RedirectToHome();

            return View(new RegisterViewModel());
        }

        // Processes the registration form submitted by the user.
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            // Checks whether the submitted registration details are valid.
            if (!ModelState.IsValid) return View(model);

            // Sends the registration details to the API.
            var result = await _api.PostAsync(ApiUrl.Build("api/Users/register",
                ("fullname", model.FullName.Trim()),
                ("email", model.Email.Trim()),
                ("password", model.Password),
                ("role", model.Role)));

            // Displays an error if registration was unsuccessful.
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View(model);
            }

            // Displays a success message and redirects to the login page.
            FlashSuccess("Registration successful! Please log in.");
            return RedirectToAction(nameof(Login));
        }

        // ---------- LOGIN ----------

        // Displays the login page.
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            // Redirects users who are already logged in.
            if (HttpContext.Session.IsLoggedIn()) return RedirectToHome();

            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        // Processes the login form submitted by the user.
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            // Checks whether the login details are valid.
            if (!ModelState.IsValid) return View(model);

            // Clears the existing session before starting a new login.
            HttpContext.Session.Clear();

            // Sends the email and password to the API for authentication.
            var result = await _api.LoginAsync(model.Email.Trim(), model.Password);

            // Displays an error if login fails or no user data is returned.
            if (!result.Success || result.Data == null)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                model.Password = string.Empty;
                return View(model);
            }

            // Stores the logged-in user's ID, role and name in the session.
            HttpContext.Session.SignIn(result.Data);

            // Redirects the user to the original page if it is a valid local URL.
            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                return LocalRedirect(model.ReturnUrl);

            // Redirects the user to the appropriate home page.
            return RedirectToHome();
        }

        // ---------- LOGOUT ----------

        // Logs the user out of the application.
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // Clears the user's session in the API.
            await _api.LogoutAsync();

            // Clears the user's session in the MVC application.
            HttpContext.Session.Clear();

            // Displays a logout confirmation and redirects to login.
            FlashSuccess("You have been logged out.");
            return RedirectToAction(nameof(Login));
        }

        // ---------- PROFILE (both roles) ----------

        // Retrieves and displays the logged-in user's profile.
        [HttpGet, SessionAuthorize]
        public async Task<IActionResult> Profile()
        {
            // Requests the user's profile details from the API.
            var result = await _api.GetAsync<UserDto>("api/Users/me");

            // Redirects the user if their session has expired.
            if (result.IsUnauthorized) return SessionExpired();

            // Handles errors when retrieving the profile.
            if (!result.Success || result.Data == null)
            {
                FlashError(result.Message);
                return RedirectToHome();
            }

            // Passes the retrieved profile details to the view.
            return View(new ProfileViewModel
            {
                FullName = result.Data.FullName,
                EmailAddress = result.Data.EmailAddress,
                UserRole = result.Data.UserRole
            });
        }

        // Updates the logged-in user's profile.
        [HttpPost, ValidateAntiForgeryToken, SessionAuthorize]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            // Keeps the user's existing role in the profile.
            model.UserRole = CurrentRole ?? string.Empty;

            // Checks whether the submitted profile details are valid.
            if (!ModelState.IsValid) return View(model);

            // Sends the updated name and email address to the API.
            var result = await _api.PutAsync(ApiUrl.Build("api/Users/me",
                ("fullName", model.FullName.Trim()),
                ("emailAddress", model.EmailAddress.Trim())));

            // Redirects the user if their session has expired.
            if (result.IsUnauthorized) return SessionExpired();

            // Displays an error if the profile update fails.
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View(model);
            }

            // Updates the user's name stored in the MVC session.
            HttpContext.Session.SetString(SessionKeys.FullName, model.FullName.Trim());

            // Displays a success message and reloads the profile page.
            FlashSuccess("Your profile has been updated.");
            return RedirectToAction(nameof(Profile));
        }

        // ---------- ACCESS DENIED ----------

        // Displays the access denied page.
        public IActionResult AccessDenied() => View();

        // ---------- REDIRECTION ----------

        // Redirects users to a landing page based on their role.
        private IActionResult RedirectToHome() =>
            CurrentRole == Roles.Organiser
                ? RedirectToAction("Dashboard", "Organiser")
                : RedirectToAction("Index", "Home");
    }
}

