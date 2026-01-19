using Exemplo.Domain.Model;
using Exemplo.Domain.Settings;
using Exemplo.Service.Config;

namespace Exemplo.Service.Queries
{
    public class BuscarCondicaoVendasQuery : BasePaginatedRequest<PagedResult<CondicaoVendaModel>>
    {
        public int? Id { get; set; }
        public string? Nome { get; set; }
    }
}
