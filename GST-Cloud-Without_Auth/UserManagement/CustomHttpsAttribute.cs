using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace UserManagement
{
    public class CustomHttpsAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext actionContext)
        {
            
            if (!String.Equals(actionContext.HttpContext.Request.Scheme, "https", StringComparison.OrdinalIgnoreCase))
            {
                actionContext.HttpContext.Response.Redirect("Error");
                return;
            }
        }
    }
}
