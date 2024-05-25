using AnjUx.Client.Services;
using System.Net;

namespace AnjUx.Server.Middlewares
{
    public class ErrorHandlingMiddleware(RequestDelegate next)
    {
        private readonly RequestDelegate _next = next;

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var code = HttpStatusCode.InternalServerError;

            var innerException = exception.InnerException ?? exception;

            switch (innerException)
            {
                case UnauthorizedAccessException:
                    code = HttpStatusCode.Unauthorized;
                    break;
                default:
                    break;
            }

            var response = new ServiceResponse<string>() { Message = innerException.Message, Success = false };
            string result = System.Text.Json.JsonSerializer.Serialize(response);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)code;
            return context.Response.WriteAsync(result);
        }
    }
}
