namespace Exemplo.Domain.Model.Dto
{
    public class VendaChatVinculoDto
    {
        public int VendaId { get; set; }
        public bool Vinculado { get; set; }
        public int? VendaWhatsappId { get; set; }
        public string? WhatsappChatId { get; set; }
    }
}
