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
        public string? Role { get; set; }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;

            if (!session.IsLoggedIn())
            {
                var request = context.HttpContext.Request;
                string? returnUrl = request.Method == HttpMethods.Get
                    ? request.Path + request.QueryString
                    : null;

                if (context.Controller is Controller c)
                    c.TempData["Error"] = "Please log in to continue.";

                context.Result = new RedirectToActionResult("Login", "Account", new { returnUrl });
                return;
            }

            if (Role != null && session.GetRole() != Role)
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
            }
        }
    }
}
