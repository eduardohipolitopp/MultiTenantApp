using System;
using System.Net;
using System.Text.Json;
using MultiTenantApp.Domain.Exceptions;
using Serilog;

namespace MultiTenantApp.Api.Middleware
{
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        public GlobalExceptionHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

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
            context.Response.ContentType = "application/json";
            
            var statusCode = HttpStatusCode.InternalServerError;
            var message = "An unexpected error occurred.";

            switch (exception)
            {
                case BusinessException businessException:
                    statusCode = HttpStatusCode.BadRequest;
                    message = businessException.Message;
                    Log.Warning(exception, "Business error occurred: {Message}", message);
                    break;
                case KeyNotFoundException:
                    statusCode = HttpStatusCode.NotFound;
                    message = "Resource not found.";
                    Log.Warning(exception, "Resource not found: {Message}", exception.Message);
                    break;
                default:
                    Log.Error(exception, "An unhandled exception occurred.");
                    break;
            }

            context.Response.StatusCode = (int)statusCode;

            var result = JsonSerializer.Serialize(new { error = message });
            return context.Response.WriteAsync(result);
        }
    }
}
