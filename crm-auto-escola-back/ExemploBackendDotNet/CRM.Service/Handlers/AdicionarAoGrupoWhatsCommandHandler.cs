using Exemplo.Domain.Model;
using Exemplo.Persistence;
using Exemplo.Service.Commands;
using Exemplo.Service.Exceptions;
using Exemplo.Service.Helpers;
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

            var vendaWhatsapp = await _context.VendaWhatsapp
                .Include(vw => vw.Venda)
                .FirstOrDefaultAsync(vw => vw.Id == request.IdVendaWhats, cancellationToken);

            if (vendaWhatsapp == null)
                throw new NotFoundException("Conversa não encontrada.");

            var contatoNovo = vendaWhatsapp.Venda?.Contato;
            if (!string.IsNullOrWhiteSpace(contatoNovo))
            {
                var conversasExistentes = await _context.GrupoVendaWhatsapp
                    .AsNoTracking()
                    .Where(gv => gv.IdGrupo == request.IdGrupoWhats)
                    .Include(gv => gv.VendaWhatsapp)
                    .ThenInclude(vw => vw.Venda)
                    .ToListAsync(cancellationToken);

                var contatoDuplicado = conversasExistentes.Any(gv =>
                    ContatoNormalization.AreEquivalent(gv.VendaWhatsapp?.Venda?.Contato, contatoNovo));

                if (contatoDuplicado)
                    throw new ConflictException("Já existe um contato equivalente neste grupo.");
            }

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
