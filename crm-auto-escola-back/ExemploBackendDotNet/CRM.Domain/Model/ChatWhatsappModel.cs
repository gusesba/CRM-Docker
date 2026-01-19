using System.Text.Json.Serialization;

namespace Exemplo.Domain.Model
{
    public class ChatWhatsappModel
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public string WhatsappChatId { get; set; } = string.Empty;
        public string NomeChat { get; set; } = string.Empty;

        [JsonIgnore]
        public UsuarioModel Usuario { get; set; }

        [JsonIgnore]
        public ICollection<MensagemWhatsappModel> Mensagens { get; set; }
    }
}
