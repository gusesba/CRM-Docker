using Exemplo.Domain.Model;
using Exemplo.Domain.Model.Dto;
using MediatR;

namespace Exemplo.Service.Commands
{
    public class RegistrarMensagemWhatsappCommand : IRequest<MensagemWhatsappModel>
    {
        public int UserId { get; set; }
        public string ChatId { get; set; } = string.Empty;
        public string ChatName { get; set; } = string.Empty;
        public WhatsappMensagemConteudoDto Message { get; set; } = new();
    }
}
