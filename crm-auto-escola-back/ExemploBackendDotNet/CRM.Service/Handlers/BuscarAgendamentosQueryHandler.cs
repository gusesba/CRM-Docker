using Exemplo.Domain.Model;
using Exemplo.Domain.Settings;
using Exemplo.Persistence;
using Exemplo.Service.Queries;
using Exemplo.Service.Security;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Exemplo.Service.Handlers
{
    public class BuscarAgendamentosQueryHandler
        : IRequestHandler<BuscarAgendamentosQuery, PagedResult<AgendamentoModel>>
    {
        private readonly ExemploDbContext _context;
        private readonly IUsuarioContextService _usuarioContextService;

        public BuscarAgendamentosQueryHandler(
            ExemploDbContext context,
            IUsuarioContextService usuarioContextService)
        {
            _context = context;
            _usuarioContextService = usuarioContextService;
        }

        public async Task<PagedResult<AgendamentoModel>> Handle(
            BuscarAgendamentosQuery request,
            CancellationToken cancellationToken)
        {
            var access = await _usuarioContextService.GetUsuarioSedeAccessAsync(cancellationToken);
            // Garantir que as datas do request sejam UTC
            if (request.DataAgendamentoDe.HasValue)
                request.DataAgendamentoDe = DateTime.SpecifyKind(request.DataAgendamentoDe.Value, DateTimeKind.Utc);

            if (request.DataAgendamentoAte.HasValue)
                request.DataAgendamentoAte = DateTime.SpecifyKind(request.DataAgendamentoAte.Value, DateTimeKind.Utc);

            IQueryable<AgendamentoModel> query = _context.Agendamento
                .Include(a => a.Venda)
                    .ThenInclude(v => v.Vendedor)
                .Include(a => a.Venda)
                    .ThenInclude(v => v.Sede)
                .Include(a => a.Venda)
                    .ThenInclude(v => v.Servico)
                .Include(a => a.Venda)
                    .ThenInclude(v => v.CondicaoVenda);

            query = query.ApplySedeFilter(access);

            // Filtros
            if (request.Id.HasValue)
                query = query.Where(a => a.Id == request.Id.Value);

            if (request.VendaId.HasValue)
                query = query.Where(a => a.VendaId == request.VendaId.Value);

            if (request.VendedorId.HasValue)
                query = query.Where(a => a.Venda.VendedorId == request.VendedorId.Value);

            if (request.DataAgendamentoDe.HasValue)
                query = query.Where(a => a.DataAgendamento >= request.DataAgendamentoDe.Value);

            if (request.DataAgendamentoAte.HasValue)
                query = query.Where(a => a.DataAgendamento <= request.DataAgendamentoAte.Value);

            if (!string.IsNullOrWhiteSpace(request.Obs))
            {
                var filtroObs = request.Obs.ToLower();
                query = query.Where(a => a.Obs.ToLower().Contains(filtroObs));
            }

            if(!string.IsNullOrWhiteSpace(request.Cliente))
            {
                var filtroCliente = request.Cliente.ToLower();
                query = query.Where(a => a.Venda.Cliente.ToLower().Contains(filtroCliente));
            }

            // Total de registros
            var totalCount = await query.CountAsync(cancellationToken);

            // Ordenação
            bool ascending = request.OrderDirection?.ToLower() != "desc";
            query = request.OrderBy?.ToLower() switch
            {
                "dataagendamento" => ascending
                    ? query.OrderBy(a => a.DataAgendamento)
                    : query.OrderByDescending(a => a.DataAgendamento),
                "id" or _ => ascending
                    ? query.OrderBy(a => a.Id)
                    : query.OrderByDescending(a => a.Id),
            };

            // Paginação
            var skip = (request.Page - 1) * request.PageSize;

            var agendamentos = await query
                .Skip(skip)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<AgendamentoModel>
            {
                Items = agendamentos,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize),
                CurrentPage = request.Page
            };
        }
    }
}
