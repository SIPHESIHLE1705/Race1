
using Microsoft.AspNetCore.Mvc;
using Race.MVC.Helpers;

namespace Race.MVC.Controllers
{
    // Base controller containing shared methods used by other controllers.
    public abstract class BaseController : Controller
    {
        // Retrieves the logged-in user's ID from the session.
        protected int? CurrentUserId => HttpContext.Session.GetInt32(SessionKeys.UserId);

        // Retrieves the logged-in user's role from the session.
        protected string? CurrentRole => HttpContext.Session.GetString(SessionKeys.UserRole);

        // Handles cases where the API returns 401 because the session has expired or restarted.
        protected IActionResult SessionExpired()
        {
            // Clears the current MVC session.
            HttpContext.Session.Clear();

            // Stores an error message to display on the login page.
            TempData["Error"] = "Your session has expired. Please log in again.";

            // Redirects the user to the login page.
            return RedirectToAction("Login", "Account");
        }

        // Stores a success message to display on the next page.
        protected void FlashSuccess(string message) => TempData["Success"] = message;

        // Stores an error message to display on the next page.
        protected void FlashError(string message) => TempData["Error"] = message;

        // Retrieves the first validation error from the model.
        // Returns a default message if no specific error message is available.
        protected string FirstModelError() =>
            ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                .FirstOrDefault(m => !string.IsNullOrWhiteSpace(m))
            ?? "Please check the information you entered.";
    }
}
