namespace Exemplo.Domain.Model.Dto
{
    public class VendaWhatsappDto
    {
        public int Id { get; set; }
        public int VendaId { get; set; }
        public string WhatsappChatId { get; set; } = string.Empty;
        public string WhatsappUserId { get; set; } = string.Empty;
        public VendaModel? Venda { get; set; }
    }
}
