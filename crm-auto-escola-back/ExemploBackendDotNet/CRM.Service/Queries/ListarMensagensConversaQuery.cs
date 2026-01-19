using Exemplo.Domain.Model;
using MediatR;

namespace Exemplo.Service.Queries
{
    public class ListarMensagensConversaQuery : IRequest<List<MensagemWhatsappModel>>
    {
        public int ChatWhatsappId { get; set; }
    }
}
