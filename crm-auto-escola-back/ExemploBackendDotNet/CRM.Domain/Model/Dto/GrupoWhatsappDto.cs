namespace Exemplo.Domain.Model.Dto
{
    public class GrupoWhatsappDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public int UsuarioId { get; set; }
        public List<GrupoWhatsappConversaDto> Conversas { get; set; } = new();
    }

    public class GrupoWhatsappConversaDto
    {
        public int Id { get; set; }
        public int VendaWhatsappId { get; set; }
        public int VendaId { get; set; }
        public VendaModel Venda { get; set; }
        public string WhatsappChatId { get; set; } = string.Empty;
        public string WhatsappUserId { get; set; } = string.Empty;
    }
}
