using Exemplo.Domain.Model;
using Exemplo.Domain.Model.Dto;
using Exemplo.Domain.Settings;
using Exemplo.Persistence;
using Exemplo.Service.Queries;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Exemplo.Service.Handlers
{
    public class BuscarSedesQueryHandler : IRequestHandler<BuscarSedesQuery, PagedResult<SedeModel>>
    {
        private readonly ExemploDbContext _context;

        public BuscarSedesQueryHandler(ExemploDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<SedeModel>> Handle(BuscarSedesQuery request, CancellationToken cancellationToken)
        {
            IQueryable<SedeModel> query = _context.Sede;

            if (!string.IsNullOrWhiteSpace(request.Nome))
            {
                var nomeFiltro = request.Nome.ToLower();
                query = query.Where(c => c.Nome.ToLower().Contains(nomeFiltro));
            }
            if (request.Ativo != null)
            {
                var AtivoFiltro = request.Ativo;
                query = query.Where(c => c.Ativo == request.Ativo);
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
                "ativo" => ascending ? query.OrderBy(c => c.Ativo) : query.OrderByDescending(c => c.Ativo),
                "dataInclusao" => ascending ? query.OrderBy(c => c.DataInclusao) : query.OrderByDescending(c => c.DataInclusao),
                "id" or _ => ascending ? query.OrderBy(c => c.Id) : query.OrderByDescending(c => c.Id),
            };

            var skip = (request.Page - 1) * request.PageSize;

            var sedes = await query
                .Skip(skip)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<SedeModel>
            {
                Items = sedes,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize),
                CurrentPage = skip + 1
            };
        }

    }
}
