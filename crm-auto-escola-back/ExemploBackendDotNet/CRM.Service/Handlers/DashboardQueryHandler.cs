using Exemplo.Domain.Model.Dto;
using Exemplo.Domain.Model.Enum;
using Exemplo.Persistence;
using Exemplo.Service.Queries;
using Exemplo.Service.Exceptions;
using Exemplo.Service.Security;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Exemplo.Service.Handlers
{
    public class DashboardQueryHandler : IRequestHandler<DashboardQuery, DashboardDto>
    {
        private readonly ExemploDbContext _context;
        private readonly IUsuarioContextService _usuarioContextService;

        public DashboardQueryHandler(
            ExemploDbContext context,
            IUsuarioContextService usuarioContextService)
        {
            _context = context;
            _usuarioContextService = usuarioContextService;
        }

        public async Task<DashboardDto> Handle(
    DashboardQuery request,
    CancellationToken cancellationToken
)
        {
            var access = await _usuarioContextService.GetUsuarioSedeAccessAsync(cancellationToken);
            // ==========================
            // QUERY BASE (SEM VENDEDOR)
            // ==========================
            var vendasBaseQuery = _context.Venda
                .AsNoTracking()
                .Include(v => v.Vendedor)
                .AsQueryable();

            vendasBaseQuery = vendasBaseQuery.ApplySedeFilter(access);

            if (request.DataInicial == default || request.DataFinal == default)
                throw new ValidationException("Período é obrigatório para o dashboard.");

            if (request.DataInicial > request.DataFinal)
                throw new ValidationException("Data inicial não pode ser maior que a data final.");

            if (request.SedeId.HasValue)
                vendasBaseQuery = vendasBaseQuery.Where(v => v.SedeId == request.SedeId);

            if (request.ServicoId.HasValue)
                vendasBaseQuery = vendasBaseQuery.Where(v => v.ServicoId == request.ServicoId);

            var dataInicialUtc = DateTime.SpecifyKind(
    request.DataInicial.Date,
    DateTimeKind.Utc
);

            var dataFinalUtc = DateTime.SpecifyKind(
                request.DataFinal.Date.AddDays(1).AddTicks(-1),
                DateTimeKind.Utc
            );

            vendasBaseQuery = vendasBaseQuery.Where(v =>
    v.DataInicial >= dataInicialUtc &&
    v.DataInicial <= dataFinalUtc
);

            // ==========================
            // QUERY FILTRADA (COM VENDEDOR)
            // ==========================
            var vendasFiltradasQuery = vendasBaseQuery;

            if (request.VendedorId.HasValue)
                vendasFiltradasQuery = vendasFiltradasQuery
                    .Where(v => v.VendedorId == request.VendedorId);

            // ==========================
            // MÉTRICAS (CARDS)
            // ==========================
            var totalLeads = await vendasFiltradasQuery.CountAsync(cancellationToken);

            var totalMatriculas = await vendasFiltradasQuery.CountAsync(
                v => v.Status == StatusEnum.VendaEfetivada,
                cancellationToken
            );

            var leadsAbertos = await vendasFiltradasQuery.CountAsync(
                v => v.Status == StatusEnum.AgendarContato
                  || v.Status == StatusEnum.StandBy,
                cancellationToken
            );

            var leadsSemSucesso = await vendasFiltradasQuery.CountAsync(
                v => v.Status == StatusEnum.OptouPelaConcorrencia
                  || v.Status == StatusEnum.NaoEnviarMais,
                cancellationToken
            );

            var totalVendas = await vendasFiltradasQuery
            .Where(v => v.Status == StatusEnum.VendaEfetivada)
            .SumAsync(v => (decimal?)v.ValorVenda, cancellationToken);

            // ==========================
            // COMPARATIVO (IGNORA VENDEDOR)
            // ==========================
            var comparativo = await vendasBaseQuery
                .GroupBy(v => new { v.VendedorId, v.Vendedor.Nome })
                .Select(g => new DashboardVendedorDto
                {
                    VendedorId = g.Key.VendedorId,
                    VendedorNome = g.Key.Nome,
                    TotalLeads = g.Count(),
                    TotalMatriculas = g.Count(v =>
                        v.Status == StatusEnum.VendaEfetivada
                    ),
                    TotalVendas = g
                        .Where(v => v.Status == StatusEnum.VendaEfetivada)
                        .Sum(v => (decimal?)v.ValorVenda) ?? 0 // ✅
                })
                .OrderByDescending(x => x.TotalVendas)
                .ToListAsync(cancellationToken);

            return new DashboardDto
            {
                TotalLeads = totalLeads,
                TotalMatriculas = totalMatriculas,
                LeadsAbertos = leadsAbertos,
                LeadsSemSucesso = leadsSemSucesso,
                TotalVendas = totalVendas, 
                ComparativoVendedores = comparativo
            };
        }

    }
}
