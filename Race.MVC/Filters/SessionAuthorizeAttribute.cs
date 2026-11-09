using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Race.MVC.Helpers;

namespace Race.MVC.Filters
{
    // Guards controllers/actions using the MVC session:
    //   not logged in  -> redirect to Login (remembering where they wanted to go)
    //   wrong role     -> redirect to Access Denied
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class SessionAuthorizeAttribute : ActionFilterAttribute
    {
        // Stores the role required to access a controller or action.
        public string? Role { get; set; }

        // Runs before the requested controller action executes.
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Gets the current user's session.
            var session = context.HttpContext.Session;

            // Checks whether the user is logged in.
            if (!session.IsLoggedIn())
            {
                // Gets details about the current request.
                var request = context.HttpContext.Request;

                // Saves the requested page as a return URL if it was a GET request.
                // This allows the user to return to the page after logging in.
                string? returnUrl = request.Method == HttpMethods.Get
                    ? request.Path + request.QueryString
                    : null;

                // Stores an error message to display on the Login page.
                if (context.Controller is Controller c)
                    c.TempData["Error"] = "Please log in to continue.";

                // Redirects the user to the Account Login action.
                context.Result = new RedirectToActionResult("Login", "Account", new { returnUrl });
                return;
            }

            // Checks whether the logged-in user's role matches the required role.
            if (Role != null && session.GetRole() != Role)
            {
                // Redirects users with the wrong role to the Access Denied page.
                context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
            }
        }
    }
}
