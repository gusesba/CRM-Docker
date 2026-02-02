using Exemplo.Domain.Model;
using Exemplo.Persistence;
using Exemplo.Service.Exceptions;
using Exemplo.Service.Queries;
using Exemplo.Service.Security;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Exemplo.Service.Handlers
{
    public class BuscarVendaByIdQueryHandler
        : IRequestHandler<BuscarVendaByIdQuery, VendaModel>
    {
        private readonly ExemploDbContext _context;
        private readonly IUsuarioContextService _usuarioContextService;

        public BuscarVendaByIdQueryHandler(
            ExemploDbContext context,
            IUsuarioContextService usuarioContextService)
        {
            _context = context;
            _usuarioContextService = usuarioContextService;
        }

        public async Task<VendaModel> Handle(
            BuscarVendaByIdQuery request,
            CancellationToken cancellationToken)
        {
            var access = await _usuarioContextService.GetUsuarioSedeAccessAsync(cancellationToken);
            IQueryable<VendaModel> query = _context.Venda
                .Include(v => v.Sede)
                .Include(v => v.Vendedor)
                .Include(v => v.Servico)
                .Include(v => v.CondicaoVenda);

            query = query.ApplySedeFilter(access);
            query = query.Where(v => v.Id == request.Id);

            var venda = await query.FirstOrDefaultAsync(cancellationToken);

            if (venda == null)
                throw new NotFoundException("Venda não encontrada.");

            return venda;
        }
    }
}
