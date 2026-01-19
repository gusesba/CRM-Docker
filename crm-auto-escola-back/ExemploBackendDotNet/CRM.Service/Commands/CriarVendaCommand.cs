using Exemplo.Domain.Model;
using Exemplo.Domain.Model.Enum;
using MediatR;

namespace Exemplo.Service.Commands
{
    public class CriarVendaCommand : IRequest<VendaModel>
    {
        public int? SedeId { get; set; }
        public int VendedorId { get; set; }
        public string Cliente { get; set; }
        public GeneroEnum? Genero { get; set; }
        public OrigemEnum? Origem { get; set; }
        public string? Email { get; set; }
        public string? Fone { get; set; }
        public string Contato { get; set; }
        public string? ComoConheceu { get; set; }
        public string? MotivoEscolha { get; set; }
        public int? ServicoId { get; set; }
        public string? Obs { get; set; }
        public int? CondicaoVendaId { get; set; }
        public StatusEnum Status { get; set; }
        public decimal? ValorVenda { get; set; }
        public string? Indicacao { get; set; }
        public string? Contrato { get; set; }
        public DateTime? DataNascimento { get; set; }
    }
}
