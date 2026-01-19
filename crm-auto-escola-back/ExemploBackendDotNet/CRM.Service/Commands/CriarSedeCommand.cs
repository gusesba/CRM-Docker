using Exemplo.Domain.Model;
using MediatR;

namespace Exemplo.Service.Commands
{
    public class CriarSedeCommand : IRequest<SedeModel>
    {
        public required string Nome {  get; set; } = string.Empty;
        public DateOnly DataInclusao { get; set; } = DateOnly.FromDateTime(DateTime.Now);
        public bool Ativo = true;
    }
}
