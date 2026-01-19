using Exemplo.Domain.Model.Dto;
using MediatR;

namespace Exemplo.Service.Commands
{
    public class SignUpCommand : IRequest<LoginDto>
    {
        public string Usuario { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
        public string Nome {  get; set; } = string.Empty;
        public bool IsAdmin { get; set; } = false;
        public int? SedeId { get; set; }
    }
}
