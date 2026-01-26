using Exemplo.Domain.Model;
using Exemplo.Domain.Settings;
using Exemplo.Service.Config;

namespace Exemplo.Service.Queries
{
    public class BuscarAgendamentosQuery : BasePaginatedRequest<PagedResult<AgendamentoModel>>
    {
        public int? Id { get; set; }

        public int? VendaId { get; set; }
        public int? VendedorId { get; set; }
        public string? Cliente { get; set; }

        public DateTime? DataAgendamentoDe { get; set; }
        public DateTime? DataAgendamentoAte { get; set; }

        public string? Obs { get; set; }
    }
}
