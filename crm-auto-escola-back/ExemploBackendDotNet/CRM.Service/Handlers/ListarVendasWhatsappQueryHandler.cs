using Exemplo.Domain.Model;
using Exemplo.Domain.Model.Dto;
using Exemplo.Persistence;
using Exemplo.Service.Exceptions;
using Exemplo.Service.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Exemplo.Service.Handlers
{
    public class ListarVendasWhatsappQueryHandler
        : IRequestHandler<ListarVendasWhatsappQuery, List<VendaWhatsappDto>>
    {
        private readonly ExemploDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ListarVendasWhatsappQueryHandler(
            ExemploDbContext context,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<List<VendaWhatsappDto>> Handle(
     ListarVendasWhatsappQuery request,
     CancellationToken cancellationToken)
        {
            var userIdValue = _httpContextAccessor.HttpContext?.User?.FindFirst("UserId")?.Value;
            if (!int.TryParse(userIdValue, out var userId))
                throw new UnauthorizedException("Usuário não autenticado.");

            var usuario = await _context.Usuario
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

            if (usuario == null)
                throw new UnauthorizedException("Usuário não autenticado.");

            IQueryable<VendaWhatsappModel> query = _context.VendaWhatsapp
                .AsNoTracking()
                .Include(vw => vw.Venda);

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
