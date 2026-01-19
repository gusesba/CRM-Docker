using Exemplo.Domain.Model.Dto;
using Exemplo.Domain.Model.Enum;
using System.Text.Json.Serialization;

namespace Exemplo.Domain.Model
{
    public class VendaModel
    {
        public int Id { get; set; }
        public int? SedeId { get; set; }
        public SedeModel? Sede { get; set; }
        public DateTime DataInicial { get; set; }
        public int VendedorId { get; set; }
        public UsuarioModel Vendedor { get; set; }
        public DateTime DataAlteracao { get; set; }
        public string Cliente { get; set; }
        public GeneroEnum? Genero { get; set; }
        public OrigemEnum? Origem { get; set; }
        public string? Email { get; set; }
        public string? Fone { get; set; }
        public string Contato { get; set; }
        public string? ComoConheceu { get; set; }
        public string? MotivoEscolha { get; set; }
        public int? ServicoId { get; set; }
        public ServicoModel? Servico { get; set; }
        public string? Obs { get; set; }
        public int? CondicaoVendaId { get; set; }
        public CondicaoVendaModel? CondicaoVenda { get; set; }
        public StatusEnum Status { get; set; }
        public decimal? ValorVenda { get; set; }
        public string? Indicacao { get; set; }
        public string? Contrato { get; set; }
        public DateTime? DataNascimento { get; set; }
        public int? VendedorAtualId { get; set; }
        public UsuarioModel VendedorAtual { get; set; }

        [JsonIgnore]
        public ICollection<AgendamentoModel> Agendamentos { get; set; }
        public VendaWhatsappModel? VendaWhatsapp { get; set; }
    }
}
