using Exemplo.Persistence;
using Exemplo.Service.Commands;
using Exemplo.Service.Exceptions;
using Exemplo.Service.Security;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Exemplo.Service.Handlers
{
    public class ExcluirVendaWhatsappCommandHandler : IRequestHandler<ExcluirVendaWhatsappCommand>
    {
        private readonly ExemploDbContext _context;
        private readonly IUsuarioContextService _usuarioContextService;

        public ExcluirVendaWhatsappCommandHandler(
            ExemploDbContext context,
            IUsuarioContextService usuarioContextService)
        {
            _context = context;
            _usuarioContextService = usuarioContextService;
        }

        public async Task Handle(ExcluirVendaWhatsappCommand request, CancellationToken cancellationToken)
        {
            var access = await _usuarioContextService.GetUsuarioSedeAccessAsync(cancellationToken);
            var vendaWhatsapp = await _context.VendaWhatsapp
                .Include(vw => vw.Venda)
                .FirstOrDefaultAsync(vw => vw.Id == request.VendaWhatsappId, cancellationToken);

            if (vendaWhatsapp == null)
            {
                throw new NotFoundException("Venda Whatsapp não encontrada.");
            }

            access.EnsureSameSede(vendaWhatsapp.Venda?.SedeId, "Venda não pertence à sua sede.");

            var gruposVenda = await _context.GrupoVendaWhatsapp
                .Where(gv => gv.IdVendaWhats == request.VendaWhatsappId)
                .ToListAsync(cancellationToken);

            if (gruposVenda.Count > 0)
            {
                _context.GrupoVendaWhatsapp.RemoveRange(gruposVenda);
            }

            _context.VendaWhatsapp.Remove(vendaWhatsapp);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
