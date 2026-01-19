using System.Net;

namespace Exemplo.Service.Exceptions
{
    public sealed class UnauthorizedException : ApiException
    {
        public UnauthorizedException(string message)
            : base(HttpStatusCode.Unauthorized, "Não autorizado", message)
        {
        }
    }
}
