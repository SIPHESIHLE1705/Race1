using Microsoft.AspNetCore.Mvc;
using Race.MVC.Helpers;

namespace Race.MVC.Controllers
{
    public abstract class BaseController : Controller
    {
        protected int? CurrentUserId => HttpContext.Session.GetInt32(SessionKeys.UserId);
        protected string? CurrentRole => HttpContext.Session.GetString(SessionKeys.UserRole);

        // Used when the API answers 401 (its session has expired or was restarted)
        protected IActionResult SessionExpired()
        {
            HttpContext.Session.Clear();
            TempData["Error"] = "Your session has expired. Please log in again.";
            return RedirectToAction("Login", "Account");
        }

        protected void FlashSuccess(string message) => TempData["Success"] = message;
        protected void FlashError(string message) => TempData["Error"] = message;

        protected string FirstModelError() =>
            ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                .FirstOrDefault(m => !string.IsNullOrWhiteSpace(m))
            ?? "Please check the information you entered.";
    }
}
