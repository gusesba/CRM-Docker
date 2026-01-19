namespace Exemplo.Domain.Model.Dto
{
    public class WhatsappMensagemDto
    {
        public int UserId { get; set; }
        public string ChatId { get; set; } = string.Empty;
        public WhatsappMensagemConteudoDto Message { get; set; } = new();
    }

    public class WhatsappMensagemConteudoDto
    {
        public string Id { get; set; } = string.Empty;
        public string? Body { get; set; }
        public bool FromMe { get; set; }
        public long Timestamp { get; set; }
        public string Type { get; set; } = string.Empty;
        public bool HasMedia { get; set; }
        public string? MediaUrl { get; set; }
    }
}
