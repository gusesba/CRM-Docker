using Exemplo.Domain.Model.Enum;

namespace Exemplo.Domain.Model.Dto
{
    public class UsuarioDto
    {
        public string Nome { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
        public int Id { get; set; } = -1;
        public StatusUsuarioEnum Status { get; set; } = StatusUsuarioEnum.Ativo;
        public int? SedeId { get; set; }
    }
}
