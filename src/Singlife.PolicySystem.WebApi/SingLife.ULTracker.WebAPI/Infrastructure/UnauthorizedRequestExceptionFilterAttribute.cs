using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SingLife.ULTracker.UseCases.Auth;

namespace SingLife.ULTracker.WebAPI.Infrastructure
{
    public class UnauthorizedRequestExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            if (context.Exception is UnauthorizedRequestException)
            {
                context.Result = new UnauthorizedResult();
            }
        }
    }
}