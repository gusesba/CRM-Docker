using Exemplo.Domain.Model;
using Exemplo.Domain.Model.Enum;
using MediatR;

namespace Exemplo.Service.Commands
{
    public class CriarGrupoWhatsCommand : IRequest<GrupoWhatsappModel>
    {
        public string Nome { get; set; }
        public int UsuarioId { get; set; }
        public StatusEnum? Status { get; set; }
        public DateTime? DataInicialDe { get; set; }
        public DateTime? DataInicialAte { get; set; }
    }
}
