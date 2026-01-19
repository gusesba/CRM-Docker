using Exemplo.Domain.Model.Dto;
using MediatR;

namespace Exemplo.Service.Commands
{
    public class VincularVendaWhatsCommand : IRequest<ChatStatusDto>
    {
        public int VendaId { get; set; }
        public string WhatsappChatId { get; set; } = String.Empty;
        public string WhatsappUserId { get; set; } = String.Empty;
    }
}
