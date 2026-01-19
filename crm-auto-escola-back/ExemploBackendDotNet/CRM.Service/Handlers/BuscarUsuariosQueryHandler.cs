using Exemplo.Domain.Model;
using Exemplo.Domain.Model.Dto;
using Exemplo.Domain.Settings;
using Exemplo.Persistence;
using Exemplo.Service.Queries;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Exemplo.Service.Handlers
{
    public class BuscarUsuariosQueryHandler : IRequestHandler<BuscarUsuariosQuery, PagedResult<UsuarioDto>>
    {
        private readonly ExemploDbContext _context;

        public BuscarUsuariosQueryHandler(ExemploDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<UsuarioDto>> Handle(BuscarUsuariosQuery request, CancellationToken cancellationToken)
        {
            IQueryable<UsuarioModel> query = _context.Usuario;

            if (!string.IsNullOrWhiteSpace(request.Nome))
            {
                var nomeFiltro = request.Nome.ToLower();
                query = query.Where(c => c.Nome.ToLower().Contains(nomeFiltro));
            }
            if (!string.IsNullOrWhiteSpace(request.Usuario))
            {
                var usuarioFiltro = request.Usuario.ToLower();
                query = query.Where(c => c.Usuario.ToLower().Contains(usuarioFiltro));
            }
            if (request.Id != null)
            {
                query = query.Where(c => c.Id == request.Id);
            }
            if(request.Status != null)
            {
                query = query.Where(c => c.Status == request.Status);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            bool ascending = request.OrderDirection?.ToLower() != "desc";
            query = request.OrderBy?.ToLower() switch
            {
                "nome" => ascending ? query.OrderBy(c => c.Nome) : query.OrderByDescending(c => c.Nome),
                "usuario" => ascending ? query.OrderBy(c => c.Usuario) : query.OrderByDescending(c => c.Usuario),
                "status" => ascending ? query.OrderBy(c => c.Status) : query.OrderByDescending(c => c.Status),
                "id" or _ => ascending ? query.OrderBy(c => c.Id) : query.OrderByDescending(c => c.Id),
            };

            var skip = (request.Page - 1) * request.PageSize;

            var usuarios = await query
                .Skip(skip)
                .Take(request.PageSize)
                .Select(u => new UsuarioDto
                {
                    Id = u.Id,
                    Nome = u.Nome,
                    Usuario = u.Usuario,
                    Status = u.Status,
                    SedeId = u.SedeId
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<UsuarioDto>
            {
                Items = usuarios,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize),
                CurrentPage = skip + 1
            };
        }

    }
}
