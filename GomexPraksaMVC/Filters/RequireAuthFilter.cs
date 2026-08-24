using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GomexPraksaMVC.Filters
{
    public class RequireAuthFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            var token = context.HttpContext.Session.GetString("auth_token");
            if (string.IsNullOrWhiteSpace(token))
            {
                context.Result = new RedirectToActionResult("Login", "Account",
                    new { returnUrl = context.HttpContext.Request.Path });
            }
        }

        public void OnActionExecuted(ActionExecutedContext context) { }
    }
}