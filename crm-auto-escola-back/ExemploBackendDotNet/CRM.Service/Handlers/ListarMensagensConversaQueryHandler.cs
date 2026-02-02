using Exemplo.Domain.Model;
using Exemplo.Persistence;
using Exemplo.Service.Queries;
using Exemplo.Service.Security;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Exemplo.Service.Handlers
{
    public class ListarMensagensConversaQueryHandler
        : IRequestHandler<ListarMensagensConversaQuery, List<MensagemWhatsappModel>>
    {
        private readonly ExemploDbContext _context;
        private readonly IUsuarioContextService _usuarioContextService;

        public ListarMensagensConversaQueryHandler(
            ExemploDbContext context,
            IUsuarioContextService usuarioContextService)
        {
            _context = context;
            _usuarioContextService = usuarioContextService;
        }

        public async Task<List<MensagemWhatsappModel>> Handle(
            ListarMensagensConversaQuery request,
            CancellationToken cancellationToken)
        {
            var access = await _usuarioContextService.GetUsuarioSedeAccessAsync(cancellationToken);

            return await _context.MensagemWhatsapp
                .AsNoTracking()
                .ApplySedeFilter(access)
                .Where(mensagem => mensagem.ChatWhatsappId == request.ChatWhatsappId)
                .OrderBy(mensagem => mensagem.Timestamp)
                .ToListAsync(cancellationToken);
        }
    }
}
