using Exemplo.Domain.Model;
using MediatR;

namespace Exemplo.Service.Commands
{
    public class CriarCondicaoVendaCommand : IRequest<CondicaoVendaModel>
    {
        public required string Nome {  get; set; } = string.Empty;
    }
}
