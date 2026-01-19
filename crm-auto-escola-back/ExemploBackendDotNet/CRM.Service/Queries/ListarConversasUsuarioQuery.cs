using Exemplo.Domain.Model;
using MediatR;

namespace Exemplo.Service.Queries
{
    public class ListarConversasUsuarioQuery : IRequest<List<ChatWhatsappModel>>
    {
        public int UsuarioId { get; set; }
    }
}
