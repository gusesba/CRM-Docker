using Exemplo.Domain.Model.Enum;
using System.Text.Json.Serialization;

namespace Exemplo.Domain.Model
{
    public class UsuarioModel
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public string SenhaHash { get; set; } = string.Empty;
        public bool IsAdmin { get; set; } = false;
        public StatusUsuarioEnum Status { get; set; } = StatusUsuarioEnum.Ativo;
        public int? SedeId { get; set; }
        public SedeModel? Sede { get; set; }

        [JsonIgnore]
        public ICollection<VendaModel> Vendas { get; set; }

        [JsonIgnore]
        public ICollection<VendaModel> VendasAtuais { get; set; }

        [JsonIgnore]
        public ICollection<GrupoWhatsappModel> GruposWhatsapp { get; set; }

        [JsonIgnore]
        public ICollection<ChatWhatsappModel> ChatsWhatsapp { get; set; }
    }
}
