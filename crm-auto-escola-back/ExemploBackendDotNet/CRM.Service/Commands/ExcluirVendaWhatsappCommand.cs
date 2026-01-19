using MediatR;

namespace Exemplo.Service.Commands
{
    public class ExcluirVendaWhatsappCommand : IRequest
    {
        public int VendaWhatsappId { get; set; }
    }
}
