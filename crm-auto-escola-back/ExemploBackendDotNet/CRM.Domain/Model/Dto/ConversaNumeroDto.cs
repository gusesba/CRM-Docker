using Exemplo.Domain.Model.Enum;

namespace Exemplo.Domain.Model.Dto
{
    public class ConversaNumeroDto
    {
        public string WhatsappChatId { get; set; } = string.Empty;
        public string Numero { get; set; } = string.Empty;
    }

    public class ConversaVinculoResultadoDto
    {
        public string WhatsappChatId { get; set; } = string.Empty;
        public WhatsStatusEnum Status { get; set; }
        public VendaModel? Venda { get; set; }
    }

    public class ConversaVinculoResumoDto
    {
        public int ConversasJaVinculadas { get; set; }
        public int ConversasVinculadas { get; set; }
        public int ConversasSemLeadEncontrado { get; set; }
    }
}
