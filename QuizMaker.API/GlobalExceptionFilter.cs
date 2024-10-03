using NLog;
using System.Net.Http;
using System.Web.Http.Filters;

namespace QuizMaker.API
{
    public class GlobalExceptionFilter : ExceptionFilterAttribute
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public override void OnException(HttpActionExecutedContext context)
        {
            var controllerName = context.ActionContext.ControllerContext.ControllerDescriptor.ControllerName;
            var actionName = context.ActionContext.ActionDescriptor.ActionName;
            var exception = context.Exception;

            Logger.Error(exception, $"Unhandled exception in {controllerName}.{actionName}: {exception.Message}");

            context.Response = context.Request.CreateErrorResponse(System.Net.HttpStatusCode.InternalServerError, "An unexpected error has occurred.");
        }
    }
}