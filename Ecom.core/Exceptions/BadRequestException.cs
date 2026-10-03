namespace Ecom.core.Exceptions
{
    /// <summary>Thrown when a request is syntactically valid but cannot be processed. Mapped to HTTP 400.</summary>
    public class BadRequestException : Exception
    {
        public BadRequestException(string message) : base(message)
        {
        }
    }
}
