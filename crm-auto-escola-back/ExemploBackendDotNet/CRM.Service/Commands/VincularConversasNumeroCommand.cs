using Exemplo.Domain.Model.Dto;
using MediatR;

namespace Exemplo.Service.Commands
{
    public class VincularConversasNumeroCommand : IRequest<List<ConversaVinculoResultadoDto>>
    {
        public List<ConversaNumeroDto> Conversas { get; set; } = new();
    }
}
