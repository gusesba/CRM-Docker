using Exemplo.Domain.Model;
using Exemplo.Persistence;
using Exemplo.Service.Commands;
using Exemplo.Service.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Exemplo.Service.Handlers
{
    public class AdicionarAoGrupoWhatsCommandHandler
        : IRequestHandler<AdicionarAoGrupoWhatsCommand, GrupoVendaWhatsappModel>
    {
        private readonly ExemploDbContext _context;

        public AdicionarAoGrupoWhatsCommandHandler(ExemploDbContext context)
        {
            _context = context;
        }

        public async Task<GrupoVendaWhatsappModel> Handle(
            AdicionarAoGrupoWhatsCommand request,
            CancellationToken cancellationToken)
        {
            // 🔎 Verifica se a venda existe
            var adicionado = await _context.GrupoVendaWhatsapp
                .FirstOrDefaultAsync(v => v.IdVendaWhats == request.IdVendaWhats && v.IdGrupo == request.IdGrupoWhats, cancellationToken);

            if (adicionado != null)
                throw new ConflictException("Já adicionado ao grupo.");

            var grupoVenda = new GrupoVendaWhatsappModel()
            {
                IdGrupo = request.IdGrupoWhats,
                IdVendaWhats = request.IdVendaWhats,
            };

            var entity = _context.GrupoVendaWhatsapp.Add(grupoVenda);
            await _context.SaveChangesAsync(cancellationToken);

            return entity.Entity;
        }
    }
}
