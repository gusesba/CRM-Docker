using Exemplo.Domain.Model;
using MediatR;

namespace Exemplo.Service.Queries
{
    public class ExemploQuery : IRequest<ExemploModel>
    {
        public ExemploQuery() { }

        public required int CampoQuery { get; set; }
    }
}
