using Exemplo.Domain.Model.Dto;
using Exemplo.Persistence;
using Exemplo.Service.Queries;
using Exemplo.Service.Security;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Exemplo.Service.Handlers
{
    public class GetVendaChatVinculoQueryHandler
        : IRequestHandler<GetVendaChatVinculoQuery, VendaChatVinculoDto>
    {
        private readonly ExemploDbContext _context;
        private readonly IUsuarioContextService _usuarioContextService;

        public GetVendaChatVinculoQueryHandler(
            ExemploDbContext context,
            IUsuarioContextService usuarioContextService)
        {
            _context = context;
            _usuarioContextService = usuarioContextService;
        }

        public async Task<VendaChatVinculoDto> Handle(
            GetVendaChatVinculoQuery request,
            CancellationToken cancellationToken)
        {
            var access = await _usuarioContextService.GetUsuarioSedeAccessAsync(cancellationToken);
            var vinculo = await _context.VendaWhatsapp
                .AsNoTracking()
                .Include(vw => vw.Venda)
                .ApplySedeFilter(access)
                .FirstOrDefaultAsync(vw => vw.VendaId == request.VendaId, cancellationToken);

            return new VendaChatVinculoDto
            {
                VendaId = request.VendaId,
                Vinculado = vinculo != null,
                VendaWhatsappId = vinculo?.Id,
                WhatsappChatId = vinculo?.WhatsappChatId
            };
        }
    }
}
