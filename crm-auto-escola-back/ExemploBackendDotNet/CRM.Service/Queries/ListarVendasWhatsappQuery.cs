using Exemplo.Domain.Model.Dto;
using MediatR;

namespace Exemplo.Service.Queries
{
    public class ListarVendasWhatsappQuery : IRequest<List<VendaWhatsappDto>>
    {
        public string? Pesquisa { get; set; }
    }
}
