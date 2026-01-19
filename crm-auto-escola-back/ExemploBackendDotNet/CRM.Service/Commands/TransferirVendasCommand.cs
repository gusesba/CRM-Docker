using MediatR;

namespace Exemplo.Service.Commands
{
    public class TransferirVendasCommand : IRequest<Unit>
    {
        public int UsuarioId { get; set; }

        public List<int> VendasIds { get; set; } = new();

        public bool Permanente { get; set; }
    }
}
