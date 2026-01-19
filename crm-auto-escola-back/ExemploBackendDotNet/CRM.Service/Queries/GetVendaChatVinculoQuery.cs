using Exemplo.Domain.Model.Dto;
using MediatR;

namespace Exemplo.Service.Queries
{
    public class GetVendaChatVinculoQuery : IRequest<VendaChatVinculoDto>
    {
        public int VendaId { get; set; }
    }
}
