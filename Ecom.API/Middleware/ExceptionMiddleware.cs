using Ecom.API.Models;
using Ecom.core.Exceptions;

namespace Ecom.API.Middleware
{
    /// <summary>Converts exceptions into the standard <see cref="ApiErrorResponse"/> envelope.</summary>
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHostEnvironment _environment;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, IHostEnvironment environment, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _environment = environment;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
            {
                // The client went away; nobody is listening for a response.
            }
            catch (Exception ex)
            {
                if (context.Response.HasStarted)
                    throw;

                var (statusCode, message) = ex switch
                {
                    NotFoundException => (StatusCodes.Status404NotFound, ex.Message),
                    BadRequestException => (StatusCodes.Status400BadRequest, ex.Message),
                    ConflictException => (StatusCodes.Status409Conflict, ex.Message),
                    _ => (StatusCodes.Status500InternalServerError,
                          _environment.IsDevelopment() ? ex.Message : "An unexpected error occurred."),
                };

                string? details = null;
                if (statusCode == StatusCodes.Status500InternalServerError)
                {
                    _logger.LogError(ex, "Unhandled exception for {Method} {Path}", context.Request.Method, context.Request.Path);
                    if (_environment.IsDevelopment())
                        details = ex.StackTrace;
                }

                context.Response.StatusCode = statusCode;
                await context.Response.WriteAsJsonAsync(new ApiErrorResponse(statusCode, message, details));
            }
        }
    }
}
