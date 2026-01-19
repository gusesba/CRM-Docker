using MediatR;

namespace Exemplo.Service.Commands
{
    public class RemoverConversasGrupoWhatsCommand : IRequest
    {
        public int IdGrupoWhats { get; set; }

        public List<int> IdsVendaWhats { get; set; } = new();
    }
}
