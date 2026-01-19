using Exemplo.Domain.Model;
using MediatR;

namespace Exemplo.Service.Queries
{
        public class BuscarVendaByIdQuery : IRequest<VendaModel>
        {
            public int Id { get; set; }
        }
}
