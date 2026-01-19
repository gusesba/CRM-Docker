using Exemplo.Domain.Model;
using MediatR;

namespace Exemplo.Service.Commands
{
    public class ExemploCommand : IRequest<ExemploModel>
    {
        public required string Campo2 {  get; set; }
        public int Campo3 { get; set; }
    }
}
