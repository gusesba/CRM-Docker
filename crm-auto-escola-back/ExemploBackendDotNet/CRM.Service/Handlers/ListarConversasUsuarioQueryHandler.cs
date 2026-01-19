using Exemplo.Domain.Model;
using Exemplo.Persistence;
using Exemplo.Service.Queries;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Exemplo.Service.Handlers
{
    public class ListarConversasUsuarioQueryHandler
        : IRequestHandler<ListarConversasUsuarioQuery, List<ChatWhatsappModel>>
    {
        private readonly ExemploDbContext _context;

        public ListarConversasUsuarioQueryHandler(ExemploDbContext context)
        {
            _context = context;
        }

        public async Task<List<ChatWhatsappModel>> Handle(
            ListarConversasUsuarioQuery request,
            CancellationToken cancellationToken)
        {
            return await _context.ChatWhatsapp
                .AsNoTracking()
                .Where(chat => chat.UsuarioId == request.UsuarioId)
                .OrderBy(chat => chat.Id)
                .ToListAsync(cancellationToken);
        }
    }
}
