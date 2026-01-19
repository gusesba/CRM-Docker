using System.Text.Json.Serialization;

namespace Exemplo.Domain.Model
{
    public class ServicoModel
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        [JsonIgnore]
        public ICollection<VendaModel> Vendas { get; set; }

    }
}
