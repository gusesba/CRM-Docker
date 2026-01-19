using Exemplo.Domain.Model.Dto;
using Exemplo.Domain.Model.Enum;

namespace Exemplo.Domain.Model
{
    public class AgendamentoModel
    {
        public int Id { get; set; }
        public int VendaId { get; set; }
        public VendaModel Venda { get; set; }
        public DateTime DataAgendamento { get; set; }
        public string Obs { get; set; }
    }
}
