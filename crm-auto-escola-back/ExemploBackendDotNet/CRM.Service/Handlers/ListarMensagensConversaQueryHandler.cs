using Exemplo.Domain.Model;
using Exemplo.Persistence;
using Exemplo.Service.Queries;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Exemplo.Service.Handlers
{
    public class ListarMensagensConversaQueryHandler
        : IRequestHandler<ListarMensagensConversaQuery, List<MensagemWhatsappModel>>
    {
        private readonly ExemploDbContext _context;

        public ListarMensagensConversaQueryHandler(ExemploDbContext context)
        {
            _context = context;
        }

        public async Task<List<MensagemWhatsappModel>> Handle(
            ListarMensagensConversaQuery request,
            CancellationToken cancellationToken)
        {
            return await _context.MensagemWhatsapp
                .AsNoTracking()
                .Where(mensagem => mensagem.ChatWhatsappId == request.ChatWhatsappId)
                .OrderBy(mensagem => mensagem.Timestamp)
                .ToListAsync(cancellationToken);
        }
    }
}
