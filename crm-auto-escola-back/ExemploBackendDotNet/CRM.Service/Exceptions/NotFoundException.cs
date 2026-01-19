using System.Net;

namespace Exemplo.Service.Exceptions
{
    public sealed class NotFoundException : ApiException
    {
        public NotFoundException(string message)
            : base(HttpStatusCode.NotFound, "Recurso não encontrado", message)
        {
        }
    }
}
