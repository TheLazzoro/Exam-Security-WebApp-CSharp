using System.Net;

namespace WebApp.ErrorHandling
{
    public class API_Exception : Exception
    {
        public HttpStatusCode StatusCode { get; }
        public string Message { get; }

        public API_Exception(HttpStatusCode statusCode, string message)
        {
            this.StatusCode = statusCode;
            this.Message = message;
        }
    }
}
