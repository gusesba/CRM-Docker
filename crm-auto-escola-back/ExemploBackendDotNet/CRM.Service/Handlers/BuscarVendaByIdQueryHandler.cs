using Exemplo.Domain.Model;
using Exemplo.Persistence;
using Exemplo.Service.Exceptions;
using Exemplo.Service.Queries;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Exemplo.Service.Handlers
{
    public class BuscarVendaByIdQueryHandler
        : IRequestHandler<BuscarVendaByIdQuery, VendaModel>
    {
        private readonly ExemploDbContext _context;

        public BuscarVendaByIdQueryHandler(ExemploDbContext context)
        {
            _context = context;
        }

        public async Task<VendaModel> Handle(
            BuscarVendaByIdQuery request,
            CancellationToken cancellationToken)
        {
            IQueryable<VendaModel> query = _context.Venda
                .Include(v => v.Sede)
                .Include(v => v.Vendedor)
                .Include(v => v.Servico)
                .Include(v => v.CondicaoVenda);

            query = query.Where(v => v.Id == request.Id);

            var venda = await query.FirstOrDefaultAsync(cancellationToken);

            if (venda == null)
                throw new NotFoundException("Venda não encontrada.");

            return venda;
        }
    }
}
