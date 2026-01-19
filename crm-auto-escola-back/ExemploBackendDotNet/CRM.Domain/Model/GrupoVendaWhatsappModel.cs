namespace Exemplo.Domain.Model
{
    public class GrupoVendaWhatsappModel
    {
        public int Id { get; set; }

        public int IdVendaWhats { get; set; }
        
        public VendaWhatsappModel VendaWhatsapp { get; set; }

        public int IdGrupo { get; set; }
        public GrupoWhatsappModel GrupoWhatsapp { get; set; }
    }
}
