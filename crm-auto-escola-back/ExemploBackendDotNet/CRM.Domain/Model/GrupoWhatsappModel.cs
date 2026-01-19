using System.Text.Json.Serialization;

namespace Exemplo.Domain.Model
{
    public class GrupoWhatsappModel
    {
        public int Id { get; set; }

        public string Nome { get; set; }

        public int UsuarioId { get; set; }

        [JsonIgnore]
        public UsuarioModel Usuario { get; set; }

        [JsonIgnore]
        public ICollection<GrupoVendaWhatsappModel> GruposVendaWhatsapp { get; set; }
    }
}
