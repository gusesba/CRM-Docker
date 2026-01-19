using Exemplo.Domain.Model;
using Exemplo.Domain.Model.Dto;
using Exemplo.Domain.Model.Enum;
using Exemplo.Persistence;
using Exemplo.Service.Commands;
using Exemplo.Service.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Exemplo.Service.Handlers
{
    public class VincularVendaWhatsCommandHandler
        : IRequestHandler<VincularVendaWhatsCommand, ChatStatusDto>
    {
        private readonly ExemploDbContext _context;

        public VincularVendaWhatsCommandHandler(ExemploDbContext context)
        {
            _context = context;
        }

        public async Task<ChatStatusDto> Handle(
            VincularVendaWhatsCommand request,
            CancellationToken cancellationToken)
        {
            // 🔎 Verifica se a venda existe
            var vendaExists = await _context.Venda
                .FirstOrDefaultAsync(v => v.Id == request.VendaId, cancellationToken);

            if (vendaExists == null)
                throw new NotFoundException("Venda não encontrada.");

            // 🔎 Verifica se já existe vínculo para esse chat/user
            var existingLink = await _context.VendaWhatsapp
                .Include(x => x.Venda)
                .FirstOrDefaultAsync(x =>
                    x.WhatsappChatId == request.WhatsappChatId &&
                    x.WhatsappUserId == request.WhatsappUserId,
                    cancellationToken);

            if (existingLink != null)
                return new ChatStatusDto()
                {
                    Status = WhatsStatusEnum.Criado,
                    Venda = existingLink.Venda
                };

            // 🔎 Verifica se a venda já está vinculada a outro chat
            var vendaAlreadyLinked = await _context.VendaWhatsapp
                .AnyAsync(x => x.VendaId == request.VendaId, cancellationToken);

            if (vendaAlreadyLinked)
                throw new ConflictException(
                    "Esta venda já está vinculada a um chat do WhatsApp."
                );

            // 🔗 Cria vínculo
            var vinculo = new VendaWhatsappModel
            {
                VendaId = request.VendaId,
                WhatsappChatId = request.WhatsappChatId,
                WhatsappUserId = request.WhatsappUserId
            };

            _context.VendaWhatsapp.Add(vinculo);
            await _context.SaveChangesAsync(cancellationToken);

            return new ChatStatusDto()
            {
                Status = WhatsStatusEnum.Criado,
                Venda = vendaExists
            };
        }
    }
}
