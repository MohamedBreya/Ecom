using System.Text.Json.Serialization;

namespace Ecom.API.Models
{
    /// <summary>Envelope for every API response that carries no payload.</summary>
    public class ApiResponse
    {
        public ApiResponse(int statusCode, string? message = null)
        {
            StatusCode = statusCode;
            Message = message ?? DefaultMessage(statusCode);
        }

        public int StatusCode { get; }
        public string Message { get; }

        public static string DefaultMessage(int statusCode) => statusCode switch
        {
            200 => "Success",
            201 => "Created",
            400 => "Bad request",
            401 => "Unauthorized",
            403 => "Forbidden",
            404 => "Not found",
            405 => "Method not allowed",
            409 => "Conflict",
            429 => "Too many requests",
            500 => "Server error",
            _ => $"Status {statusCode}",
        };
    }

    /// <summary>Envelope for successful responses that carry a payload in <see cref="Data"/>.</summary>
    public class ApiResponse<T> : ApiResponse
    {
        public ApiResponse(int statusCode, T data, string? message = null) : base(statusCode, message)
        {
            Data = data;
        }

        public T Data { get; }
    }

    /// <summary>Envelope for every error response.</summary>
    public class ApiErrorResponse : ApiResponse
    {
        public ApiErrorResponse(int statusCode, string? message = null, string? details = null,
            IDictionary<string, string[]>? errors = null) : base(statusCode, message)
        {
            Details = details;
            Errors = errors;
        }

        /// <summary>Stack trace; only populated in the Development environment.</summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Details { get; }

        /// <summary>Field name to validation messages; only populated for validation failures.</summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, string[]>? Errors { get; }
    }
}
