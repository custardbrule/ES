using System.Net;
using System.Text.Json;
using RequestValidatior;
using Seed;

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
            context.Response.ContentType = "application/json";

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
            };

            switch (exception)
            {
                case ValidationError validationError:
                    context.Response.StatusCode = validationError.Status;
                    await context.Response.WriteAsync(JsonSerializer.Serialize(validationError, jsonOptions));
                    break;

                case BussinessException bussinessEx:
                    context.Response.StatusCode = bussinessEx.HttpCode;
                    await context.Response.WriteAsync(JsonSerializer.Serialize(new
                    {
                        statusCode = bussinessEx.HttpCode,
                        message = bussinessEx.Message
                    }, jsonOptions));
                    break;

                case UnauthorizedAccessException:
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    await context.Response.WriteAsync(JsonSerializer.Serialize(new
                    {
                        statusCode = (int)HttpStatusCode.Unauthorized,
                        message = "Unauthorized access"
                    }, jsonOptions));
                    break;

                default:
                    _logger.LogError(exception, "An unhandled exception occurred");
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    await context.Response.WriteAsync(JsonSerializer.Serialize(new
                    {
                        statusCode = (int)HttpStatusCode.InternalServerError,
                        message = "An internal server error occurred"
                    }, jsonOptions));
                    break;
            }
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
