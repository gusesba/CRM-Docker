using Exemplo.Domain.Model;
using Exemplo.Domain.Model.Dto;
using MediatR;

namespace Exemplo.Service.Queries
{
    public class GetVendaByWhatsappQuery : IRequest<ChatStatusDto>
    {
        public string WhatsappChatId { get; set; } = string.Empty;
        public string WhatsappUserId { get; set; } = string.Empty;
        public string Contato { get; set; } = string.Empty;
    }
}
