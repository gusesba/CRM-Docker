using Exemplo.Domain.Model;
using Exemplo.Persistence;
using Exemplo.Service.Queries;
using Exemplo.Service.Security;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Exemplo.Service.Handlers
{
    public class ListarConversasUsuarioQueryHandler
        : IRequestHandler<ListarConversasUsuarioQuery, List<ChatWhatsappModel>>
    {
        private readonly ExemploDbContext _context;
        private readonly IUsuarioContextService _usuarioContextService;

        public ListarConversasUsuarioQueryHandler(
            ExemploDbContext context,
            IUsuarioContextService usuarioContextService)
        {
            _context = context;
            _usuarioContextService = usuarioContextService;
        }

        public async Task<List<ChatWhatsappModel>> Handle(
            ListarConversasUsuarioQuery request,
            CancellationToken cancellationToken)
        {
            var access = await _usuarioContextService.GetUsuarioSedeAccessAsync(cancellationToken);

            return await _context.ChatWhatsapp
                .AsNoTracking()
                .ApplySedeFilter(access)
                .Where(chat => chat.UsuarioId == request.UsuarioId)
                .OrderBy(chat => chat.Id)
                .ToListAsync(cancellationToken);
        }
    }
}
