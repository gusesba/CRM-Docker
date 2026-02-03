using Exemplo.Domain.Model;
using Exemplo.Domain.Model.Dto;
using Exemplo.Persistence;
using Exemplo.Service.Queries;
using Exemplo.Service.Security;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Exemplo.Service.Handlers
{
    public class ListarVendasWhatsappQueryHandler
        : IRequestHandler<ListarVendasWhatsappQuery, List<VendaWhatsappDto>>
    {
        private readonly ExemploDbContext _context;
        private readonly IUsuarioContextService _usuarioContextService;

        public ListarVendasWhatsappQueryHandler(
            ExemploDbContext context,
            IUsuarioContextService usuarioContextService)
        {
            _context = context;
            _usuarioContextService = usuarioContextService;
        }

        public async Task<List<VendaWhatsappDto>> Handle(
     ListarVendasWhatsappQuery request,
     CancellationToken cancellationToken)
        {
            var access = await _usuarioContextService.GetUsuarioSedeAccessAsync(cancellationToken);
            var userId = access.UsuarioId;

            IQueryable<VendaWhatsappModel> query = _context.VendaWhatsapp
                .AsNoTracking()
                .Include(vw => vw.Venda);

            query = query.ApplySedeFilter(access);

            query = query.Where(vw =>
                vw.Venda != null &&
                (vw.Venda.VendedorId == userId || vw.Venda.VendedorAtualId == userId));

            if (!string.IsNullOrWhiteSpace(request.Pesquisa))
            {
                var filtro = request.Pesquisa.ToLower();
                query = query.Where(vw =>
                    vw.Venda != null &&
                    ((vw.Venda.Cliente ?? string.Empty).ToLower().Contains(filtro) ||
                     (vw.Venda.Contato ?? string.Empty).ToLower().Contains(filtro)));
            }

            return await query
                .Select(vw => new VendaWhatsappDto
                {
                    Id = vw.Id,
                    VendaId = vw.VendaId,
                    WhatsappChatId = vw.WhatsappChatId,
                    WhatsappUserId = vw.WhatsappUserId,
                    Venda = vw.Venda
                })
                .ToListAsync(cancellationToken);
        }
    }
}
