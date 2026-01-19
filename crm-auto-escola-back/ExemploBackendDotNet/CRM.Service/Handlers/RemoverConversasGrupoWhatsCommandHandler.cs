using Exemplo.Persistence;
using Exemplo.Service.Commands;
using Exemplo.Service.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Exemplo.Service.Handlers
{
    public class RemoverConversasGrupoWhatsCommandHandler : IRequestHandler<RemoverConversasGrupoWhatsCommand>
    {
        private readonly ExemploDbContext _context;

        public RemoverConversasGrupoWhatsCommandHandler(ExemploDbContext context)
        {
            _context = context;
        }

        public async Task Handle(RemoverConversasGrupoWhatsCommand request, CancellationToken cancellationToken)
        {
            if (request.IdsVendaWhats == null || request.IdsVendaWhats.Count == 0)
            {
                throw new ValidationException("Nenhuma conversa informada para remover.");
            }

            var grupoExists = await _context.GrupoWhatsapp
                .AnyAsync(g => g.Id == request.IdGrupoWhats, cancellationToken);

            if (!grupoExists)
            {
                throw new NotFoundException("Grupo não encontrado.");
            }

            var conversas = await _context.GrupoVendaWhatsapp
                .Where(gv => gv.IdGrupo == request.IdGrupoWhats
                    && request.IdsVendaWhats.Contains(gv.IdVendaWhats))
                .ToListAsync(cancellationToken);

            if (conversas.Count == 0)
            {
                throw new NotFoundException("Conversas não encontradas no grupo.");
            }

            _context.GrupoVendaWhatsapp.RemoveRange(conversas);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
