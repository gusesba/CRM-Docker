using Exemplo.Domain.Model;
using Exemplo.Domain.Settings;
using Exemplo.Persistence;
using Exemplo.Service.Queries;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Exemplo.Service.Handlers
{
    public class BuscarCondicaoVendasQueryHandler : IRequestHandler<BuscarCondicaoVendasQuery, PagedResult<CondicaoVendaModel>>
    {
        private readonly ExemploDbContext _context;

        public BuscarCondicaoVendasQueryHandler(ExemploDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<CondicaoVendaModel>> Handle(BuscarCondicaoVendasQuery request, CancellationToken cancellationToken)
        {
            IQueryable<CondicaoVendaModel> query = _context.CondicaoVenda;

            if (!string.IsNullOrWhiteSpace(request.Nome))
            {
                var nomeFiltro = request.Nome.ToLower();
                query = query.Where(c => c.Nome.ToLower().Contains(nomeFiltro));
            }
            if (request.Id != null)
            {
                query = query.Where(c => c.Id == request.Id);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            bool ascending = request.OrderDirection?.ToLower() != "desc";
            query = request.OrderBy?.ToLower() switch
            {
                "nome" => ascending ? query.OrderBy(c => c.Nome) : query.OrderByDescending(c => c.Nome),
                "id" or _ => ascending ? query.OrderBy(c => c.Id) : query.OrderByDescending(c => c.Id),
            };

            var skip = (request.Page - 1) * request.PageSize;

            var condicaoVendas = await query
                .Skip(skip)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<CondicaoVendaModel>
            {
                Items = condicaoVendas,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize),
                CurrentPage = skip + 1
            };
        }
            
    }
}
