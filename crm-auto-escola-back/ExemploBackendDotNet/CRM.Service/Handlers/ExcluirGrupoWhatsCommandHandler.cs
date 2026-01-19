using Exemplo.Persistence;
using Exemplo.Service.Commands;
using Exemplo.Service.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Exemplo.Service.Handlers
{
    public class ExcluirGrupoWhatsCommandHandler : IRequestHandler<ExcluirGrupoWhatsCommand>
    {
        private readonly ExemploDbContext _context;

        public ExcluirGrupoWhatsCommandHandler(ExemploDbContext context)
        {
            _context = context;
        }

        public async Task Handle(ExcluirGrupoWhatsCommand request, CancellationToken cancellationToken)
        {
            var grupo = await _context.GrupoWhatsapp
                .FirstOrDefaultAsync(g => g.Id == request.GrupoId, cancellationToken);

            if (grupo == null)
            {
                throw new NotFoundException("Grupo não encontrado.");
            }

            var gruposVenda = await _context.GrupoVendaWhatsapp
                .Where(gv => gv.IdGrupo == request.GrupoId)
                .ToListAsync(cancellationToken);

            if (gruposVenda.Count > 0)
            {
                _context.GrupoVendaWhatsapp.RemoveRange(gruposVenda);
            }

            _context.GrupoWhatsapp.Remove(grupo);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
