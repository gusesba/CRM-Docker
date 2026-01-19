using System.Net;

namespace Exemplo.Service.Exceptions
{
    public sealed class ValidationException : ApiException
    {
        public ValidationException(string message)
            : base(HttpStatusCode.BadRequest, "Dados inválidos", message)
        {
        }
    }
}
