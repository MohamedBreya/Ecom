namespace Ecom.core.Exceptions
{
    /// <summary>Thrown when an operation conflicts with the current state of a resource. Mapped to HTTP 409.</summary>
    public class ConflictException : Exception
    {
        public ConflictException(string message) : base(message)
        {
        }
    }
}
