using Ecom.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Text.Json;
using System.Threading.RateLimiting;

namespace Ecom.API.Extensions
{
    public static class ApiServiceCollectionExtensions
    {
        public const string CorsPolicyName = "Frontend";

        /// <summary>Returns validation failures in the standard <see cref="ApiErrorResponse"/> envelope, keyed by camelCase field name.</summary>
        public static IServiceCollection AddApiBehavior(this IServiceCollection services)
        {
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState
                        .Where(entry => entry.Value?.Errors.Count > 0)
                        .ToDictionary(
                            entry => JsonNamingPolicy.CamelCase.ConvertName(entry.Key),
                            entry => entry.Value!.Errors.Select(e => e.ErrorMessage).ToArray());

                    return new BadRequestObjectResult(new ApiErrorResponse(400, "Validation failed.", errors: errors));
                };
            });
            return services;
        }

        /// <summary>Per-client-IP fixed window limit. Configure with <c>RateLimiting:PermitLimit</c> and <c>RateLimiting:WindowSeconds</c>.</summary>
        public static IServiceCollection AddApiRateLimiting(this IServiceCollection services, IConfiguration configuration)
        {
            var permitLimit = configuration.GetValue("RateLimiting:PermitLimit", 100);
            var windowSeconds = configuration.GetValue("RateLimiting:WindowSeconds", 60);

            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = permitLimit,
                            Window = TimeSpan.FromSeconds(windowSeconds),
                            QueueLimit = 0,
                        }));

                options.OnRejected = async (context, cancellationToken) =>
                {
                    var response = context.HttpContext.Response;
                    response.StatusCode = StatusCodes.Status429TooManyRequests;
                    if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                        response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();

                    await response.WriteAsJsonAsync(
                        new ApiErrorResponse(429, "Too many requests. Please try again later."), cancellationToken);
                };
            });
            return services;
        }

        /// <summary>Allows the browser origins listed in <c>Cors:AllowedOrigins</c> to call the API.</summary>
        public static IServiceCollection AddFrontendCors(this IServiceCollection services, IConfiguration configuration)
        {
            var origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

            services.AddCors(options => options.AddPolicy(CorsPolicyName, policy =>
                policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod()));
            return services;
        }
    }
}
