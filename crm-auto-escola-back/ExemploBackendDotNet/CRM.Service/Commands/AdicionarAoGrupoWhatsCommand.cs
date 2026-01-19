using Exemplo.Domain.Model;
using MediatR;

namespace Exemplo.Service.Commands
{
    public class AdicionarAoGrupoWhatsCommand : IRequest<GrupoVendaWhatsappModel>
    {
        public int IdGrupoWhats { get; set; }

        public int IdVendaWhats { get; set; }
    }
}
