using RequestValidatior;
using System.Net;
using System.Text.Json;

namespace User.Api.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
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

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            _logger.LogError(exception, "An unhandled exception occurred");

            context.Response.ContentType = "application/json";

            var (statusCode, message, errors) = exception switch
            {
                ValidationError validationError => (
                    HttpStatusCode.BadRequest,
                    "Validation failed",
                    validationError.Errors
                ),
                InvalidOperationException invalidOpEx => (
                    HttpStatusCode.NotFound,
                    invalidOpEx.Message,
                    null
                ),
                UnauthorizedAccessException => (
                    HttpStatusCode.Unauthorized,
                    "Unauthorized access",
                    null
                ),
                _ => (
                    HttpStatusCode.InternalServerError,
                    "An internal server error occurred",
                    null
                )
            };

            context.Response.StatusCode = (int)statusCode;

            var response = new
            {
                statusCode = (int)statusCode,
                message,
                errors
            };

            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(jsonResponse);
        }
    }

    public static class ExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }
}
