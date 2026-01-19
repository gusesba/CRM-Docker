using Exemplo.Domain.Model;
using Exemplo.Domain.Model.Dto;
using Exemplo.Domain.Settings;
using Exemplo.Persistence;
using Exemplo.Service.Queries;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Exemplo.Service.Handlers
{
    public class BuscarServicosQueryHandler : IRequestHandler<BuscarServicosQuery, PagedResult<ServicoModel>>
    {
        private readonly ExemploDbContext _context;

        public BuscarServicosQueryHandler(ExemploDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<ServicoModel>> Handle(BuscarServicosQuery request, CancellationToken cancellationToken)
        {
            IQueryable<ServicoModel> query = _context.Servico;

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

            var servicos = await query
                .Skip(skip)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<ServicoModel>
            {
                Items = servicos,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize),
                CurrentPage = skip + 1
            };
        }

    }
}
