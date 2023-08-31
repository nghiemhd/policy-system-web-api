using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SingLife.PolicySystem.Shared.Configuration
{
    public class ValidateRequestContentLengthAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext actionContext)
        {
            var config = actionContext.HttpContext.RequestServices.GetService<IConfiguration>();

            var allowedRequestContentLength = config.GetValue<int>("MaxAllowedRequestContentLength");

            if (actionContext.HttpContext.Request.ContentLength > allowedRequestContentLength)
            {
                actionContext.Result = new BadRequestObjectResult("Request exceeded allowed size limit.");
            }
        }
    }
}
