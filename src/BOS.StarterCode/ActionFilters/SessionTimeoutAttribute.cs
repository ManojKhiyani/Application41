using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
namespace BOS.StarterCode.ActionFilters
{
    public class SessionTimeoutAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.HttpContext.Session.GetString("ModuleOperations") == null)
            {
                context.Result = new RedirectResult("~/Auth/Signout");
                return;
            }
            base.OnActionExecuting(context);
        }
    }
}