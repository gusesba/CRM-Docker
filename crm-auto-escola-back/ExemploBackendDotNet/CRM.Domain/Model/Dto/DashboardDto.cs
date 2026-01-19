namespace Exemplo.Domain.Model.Dto
{
    public class DashboardDto
    {
        public int TotalLeads { get; set; }
        public int TotalMatriculas { get; set; }
        public int LeadsAbertos { get; set; }
        public int LeadsSemSucesso { get; set; }

        public decimal? TotalVendas { get; set; } 

        public List<DashboardVendedorDto> ComparativoVendedores { get; set; } = new();
    }

    public class DashboardVendedorDto
    {
        public int VendedorId { get; set; }
        public string VendedorNome { get; set; } = string.Empty;

        public int TotalLeads { get; set; }
        public int TotalMatriculas { get; set; }

        public decimal TotalVendas { get; set; }
    }
}
