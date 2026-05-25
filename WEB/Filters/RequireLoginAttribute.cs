using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WEB.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class RequireLoginAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var controllerName = context.RouteData.Values["controller"]?.ToString();

        // Permitir acceso sin sesion solo al controlador de Auth
        if (controllerName == "Auth")
        {
            base.OnActionExecuting(context);
            return;
        }

        var isLoggedIn = context.HttpContext.Session.GetString("IsLoggedIn");

        if (isLoggedIn != "true")
        {
            context.Result = new RedirectToActionResult("Login", "Auth", null);
            return;
        }

        base.OnActionExecuting(context);
    }
}
