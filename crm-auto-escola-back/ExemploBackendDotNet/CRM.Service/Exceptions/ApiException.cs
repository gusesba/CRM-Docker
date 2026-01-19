using System.Net;

namespace Exemplo.Service.Exceptions
{
    public class ApiException : Exception
    {
        public ApiException(HttpStatusCode statusCode, string title, string message)
            : base(message)
        {
            StatusCode = (int)statusCode;
            Title = title;
        }

        public int StatusCode { get; }
        public string Title { get; }
    }
}
