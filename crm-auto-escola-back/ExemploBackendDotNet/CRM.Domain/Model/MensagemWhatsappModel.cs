using System.Text.Json.Serialization;

namespace Exemplo.Domain.Model
{
    public class MensagemWhatsappModel
    {
        public int Id { get; set; }
        public int ChatWhatsappId { get; set; }
        public string MensagemId { get; set; } = string.Empty;
        public string? Body { get; set; }
        public bool FromMe { get; set; }
        public long Timestamp { get; set; }
        public string Type { get; set; } = string.Empty;
        public bool HasMedia { get; set; }
        public string? MediaUrl { get; set; }

        [JsonIgnore]
        public ChatWhatsappModel ChatWhatsapp { get; set; }
    }
}
