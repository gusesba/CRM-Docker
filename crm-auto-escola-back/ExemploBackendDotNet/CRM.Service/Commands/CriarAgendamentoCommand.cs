using Exemplo.Domain.Model;
using MediatR;

namespace Exemplo.Service.Commands
{
    public class CriarAgendamentoCommand : IRequest<AgendamentoModel>
    {
        public int VendaId { get; set; }
        public DateTime DataAgendamento { get; set; }
        public string Obs { get; set; }
    }
}
