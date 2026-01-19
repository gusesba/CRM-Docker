using Exemplo.Domain.Model;
using Exemplo.Domain.Model.Enum;
using Exemplo.Domain.Settings;
using Exemplo.Service.Config;

namespace Exemplo.Service.Queries
{
    public class BuscarVendasQuery : BasePaginatedRequest<PagedResult<VendaModel>>
    {
        public int? Id { get; set; }

        public int? SedeId { get; set; }

        public string? Vendedor { get; set; }
        public int? VendedorId { get; set; }
        public string? VendedorAtual { get; set; }
        public int? VendedorAtualId { get; set; }

        public int? ServicoId { get; set; }

        public int? CondicaoVendaId { get; set; }

        public List<StatusEnum>? Status { get; set; }

        public GeneroEnum? Genero { get; set; }

        public OrigemEnum? Origem { get; set; }

        public string? Cliente { get; set; }

        public string? Email { get; set; }

        public string? Fone { get; set; }

        public string? Contato { get; set; }

        public DateTime? DataInicialDe { get; set; }
        public DateTime? DataInicialAte { get; set; }

        public decimal? ValorMinimo { get; set; }
        public decimal? ValorMaximo { get; set; }

        public int? NaoVendedorAtual { get; set; }
    }
}
