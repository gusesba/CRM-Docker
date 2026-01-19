using Exemplo.Domain.Model;
using Exemplo.Domain.Settings;
using Exemplo.Service.Config;

namespace Exemplo.Service.Queries
{
    public class BuscarServicosQuery : BasePaginatedRequest<PagedResult<ServicoModel>>
    {
        public int? Id { get; set; }
        public string? Nome { get; set; }
    }
}
