using Exemplo.Domain.Model;
using Exemplo.Domain.Settings;
using Exemplo.Persistence;
using Exemplo.Service.Queries;
using Exemplo.Service.Security;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Exemplo.Service.Handlers
{
    public class BuscarVendasQueryHandler
        : IRequestHandler<BuscarVendasQuery, PagedResult<VendaModel>>
    {
        private readonly ExemploDbContext _context;
        private readonly IUsuarioContextService _usuarioContextService;

        public BuscarVendasQueryHandler(
            ExemploDbContext context,
            IUsuarioContextService usuarioContextService)
        {
            _context = context;
            _usuarioContextService = usuarioContextService;
        }

        public async Task<PagedResult<VendaModel>> Handle(
            BuscarVendasQuery request,
            CancellationToken cancellationToken)
        {
            var access = await _usuarioContextService.GetUsuarioSedeAccessAsync(cancellationToken);

            IQueryable<VendaModel> query = _context.Venda
                .Include(v => v.Sede)
                .Include(v => v.Vendedor)
                .Include(v => v.Servico)
                .Include(v => v.CondicaoVenda);

            query = query.ApplySedeFilter(access);

            // Filtros
            if (request.Id.HasValue)
                query = query.Where(v => v.Id == request.Id.Value);

            if (request.SedeId.HasValue)
                query = query.Where(v => v.SedeId == request.SedeId.Value);

            if (request.ServicoId.HasValue)
                query = query.Where(v => v.ServicoId == request.ServicoId.Value);

            if (request.CondicaoVendaId.HasValue)
                query = query.Where(v => v.CondicaoVendaId == request.CondicaoVendaId.Value);

            if (request.Status != null && request.Status.Any())
            {
                query = query.Where(a => request.Status.Contains(a.Status));
            }

            if (request.Genero.HasValue)
                query = query.Where(v => v.Genero == request.Genero.Value);

            if (request.Origem.HasValue)
                query = query.Where(v => v.Origem == request.Origem.Value);

            if (request.VendedorId.HasValue)
                query = query.Where(v => v.VendedorId == request.VendedorId.Value);

            if (request.VendedorAtualId.HasValue)
                query = query.Where(v => v.VendedorAtualId == request.VendedorAtualId.Value);

            if (!string.IsNullOrWhiteSpace(request.Vendedor))
            {
                var filtro = request.Vendedor.ToLower();
                query = query.Where(v => v.Vendedor.Nome.ToLower().Contains(filtro));
            }

            if (!string.IsNullOrWhiteSpace(request.VendedorAtual))
            {
                var filtro = request.VendedorAtual.ToLower();
                query = query.Where(v => v.VendedorAtual.Nome.ToLower().Contains(filtro));
            }

            if (!string.IsNullOrWhiteSpace(request.Cliente))
            {
                var filtro = request.Cliente.ToLower();
                query = query.Where(v => v.Cliente.ToLower().Contains(filtro));
            }

            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                var filtro = request.Email.ToLower();
                query = query.Where(v => v.Email.ToLower().Contains(filtro));
            }

            if (!string.IsNullOrWhiteSpace(request.Fone))
            {
                var filtro = request.Fone.ToLower();
                query = query.Where(v => v.Fone.ToLower().Contains(filtro));
            }

            if (!string.IsNullOrWhiteSpace(request.Contato))
            {
                var filtro = request.Contato.ToLower();
                query = query.Where(v => v.Contato.ToLower().Contains(filtro));
            }

            if (request.DataInicialDe.HasValue)
                query = query.Where(v => v.DataInicial >= request.DataInicialDe.Value);

            if (request.DataInicialAte.HasValue)
                query = query.Where(v => v.DataInicial <= request.DataInicialAte.Value);

            if (request.ValorMinimo.HasValue)
                query = query.Where(v => v.ValorVenda >= request.ValorMinimo.Value);

            if (request.ValorMaximo.HasValue)
                query = query.Where(v => v.ValorVenda <= request.ValorMaximo.Value);

            if (request.NaoVendedorAtual.HasValue)
                query = query.Where(v => v.VendedorAtualId != request.NaoVendedorAtual.Value);

            // Total
            var totalCount = await query.CountAsync(cancellationToken);

            // Ordenação
            bool ascending = request.OrderDirection?.ToLower() != "desc";
            query = request.OrderBy?.ToLower() switch
            {
                "cliente" => ascending ? query.OrderBy(v => v.Cliente) : query.OrderByDescending(v => v.Cliente),
                "datainicial" => ascending ? query.OrderBy(v => v.DataInicial) : query.OrderByDescending(v => v.DataInicial),
                "valorvenda" => ascending ? query.OrderBy(v => v.ValorVenda) : query.OrderByDescending(v => v.ValorVenda),
                "status" => ascending ? query.OrderBy(v => v.Status) : query.OrderByDescending(v => v.Status),
                "id" or _ => ascending ? query.OrderBy(v => v.Id) : query.OrderByDescending(v => v.Id),
            };

            // Paginação
            var skip = (request.Page - 1) * request.PageSize;

            var vendas = await query
                .Skip(skip)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<VendaModel>
            {
                Items = vendas,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize),
                CurrentPage = request.Page
            };
        }
    }
}
