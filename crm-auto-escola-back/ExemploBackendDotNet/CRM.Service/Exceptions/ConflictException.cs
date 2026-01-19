using System.Net;

namespace Exemplo.Service.Exceptions
{
    public sealed class ConflictException : ApiException
    {
        public ConflictException(string message)
            : base(HttpStatusCode.Conflict, "Conflito", message)
        {
        }
    }
}
