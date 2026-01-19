using MediatR;

namespace Exemplo.Service.Commands
{
    public class ExcluirGrupoWhatsCommand : IRequest
    {
        public int GrupoId { get; set; }
    }
}
