using Exemplo.Domain.Model.Dto;
using MediatR;

namespace Exemplo.Service.Queries
{
    public class LoginQuery : IRequest<LoginDto>
    {
        public string Usuario { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
    }
}
