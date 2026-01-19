using Exemplo.Domain.Model.Dto;
using Exemplo.Domain.Model.Enum;
using MediatR;

namespace Exemplo.Service.Commands
{
    public class EditarUsuarioCommand : IRequest<UsuarioDto>
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
        public string? Senha { get; set; }
        public bool IsAdmin { get; set; }
        public StatusUsuarioEnum Status { get; set; } = StatusUsuarioEnum.Ativo;
        public int? SedeId { get; set; }
    }
}
