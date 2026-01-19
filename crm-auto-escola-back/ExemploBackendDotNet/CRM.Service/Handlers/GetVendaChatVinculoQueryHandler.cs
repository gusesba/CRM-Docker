using Exemplo.Domain.Model.Dto;
using Exemplo.Persistence;
using Exemplo.Service.Queries;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Exemplo.Service.Handlers
{
    public class GetVendaChatVinculoQueryHandler
        : IRequestHandler<GetVendaChatVinculoQuery, VendaChatVinculoDto>
    {
        private readonly ExemploDbContext _context;

        public GetVendaChatVinculoQueryHandler(ExemploDbContext context)
        {
            _context = context;
        }

        public async Task<VendaChatVinculoDto> Handle(
            GetVendaChatVinculoQuery request,
            CancellationToken cancellationToken)
        {
            var vinculo = await _context.VendaWhatsapp
                .AsNoTracking()
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
