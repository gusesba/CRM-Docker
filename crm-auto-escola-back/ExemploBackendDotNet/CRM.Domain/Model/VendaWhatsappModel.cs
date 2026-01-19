
using System.Text.Json.Serialization;

namespace Exemplo.Domain.Model
{
    public class VendaWhatsappModel
    {
        public int Id { get; set; }

        public int VendaId { get; set; }
        [JsonIgnore]
        public VendaModel Venda { get; set; }

        public string WhatsappChatId { get; set; } = String.Empty;
        public string WhatsappUserId { get; set; } = String.Empty;

        [JsonIgnore]
        public ICollection<GrupoVendaWhatsappModel> GruposVendaWhatsapp { get; set; }
    }
}
