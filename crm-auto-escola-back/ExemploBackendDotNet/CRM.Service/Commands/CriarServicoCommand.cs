using Exemplo.Domain.Model;
using MediatR;

namespace Exemplo.Service.Commands
{
    public class CriarServicoCommand : IRequest<ServicoModel>
    {
        public required string Nome {  get; set; } = string.Empty;
    }
}
