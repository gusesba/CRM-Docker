using Exemplo.Domain.Model.Dto;
using MediatR;

namespace Exemplo.Service.Queries
{
    public class DashboardQuery : IRequest<DashboardDto>
    {
        public int? SedeId { get; set; }
        public int? VendedorId { get; set; }
        public int? ServicoId { get; set; }

        public DateTime DataInicial { get; set; }
        public DateTime DataFinal { get; set; }
    }
}
