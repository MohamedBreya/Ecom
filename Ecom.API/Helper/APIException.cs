namespace Ecom.API.Helper
{
    public class ApiException : ResponseAPI
    {
        public ApiException(int statusCode, string message = null,string details = null) : base(statusCode, message)
        {
            Details = details;
        }

        public string Details { get; set; }
    }
}
