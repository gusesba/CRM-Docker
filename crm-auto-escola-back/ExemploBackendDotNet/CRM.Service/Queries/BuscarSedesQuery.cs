using Exemplo.Domain.Model;
using Exemplo.Domain.Settings;
using Exemplo.Service.Config;

namespace Exemplo.Service.Queries
{
    public class BuscarSedesQuery : BasePaginatedRequest<PagedResult<SedeModel>>
    {
        public int? Id { get; set; }
        public string? Nome { get; set; }
        public bool? Ativo {  get; set; }
    }
}
