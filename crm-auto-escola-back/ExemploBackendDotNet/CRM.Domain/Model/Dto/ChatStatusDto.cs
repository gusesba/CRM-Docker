using Exemplo.Domain.Model.Enum;

namespace Exemplo.Domain.Model.Dto
{
    public class ChatStatusDto
    {
        public WhatsStatusEnum Status { get; set; }
        public VendaModel Venda { get; set; }
    }
}
