using Exemplo.Domain.Model;
using Exemplo.Domain.Model.Dto;
using Exemplo.Domain.Model.Enum;
using Exemplo.Persistence;
using Exemplo.Service.Commands;
using Exemplo.Service.Exceptions;
using Exemplo.Service.Security;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Exemplo.Service.Handlers
{
    public class VincularVendaWhatsCommandHandler
        : IRequestHandler<VincularVendaWhatsCommand, ChatStatusDto>
    {
        private readonly ExemploDbContext _context;
        private readonly IUsuarioContextService _usuarioContextService;

        public VincularVendaWhatsCommandHandler(
            ExemploDbContext context,
            IUsuarioContextService usuarioContextService)
        {
            _context = context;
            _usuarioContextService = usuarioContextService;
        }

        public async Task<ChatStatusDto> Handle(
            VincularVendaWhatsCommand request,
            CancellationToken cancellationToken)
        {
            var access = await _usuarioContextService.GetUsuarioSedeAccessAsync(cancellationToken);

            // 🔎 Verifica se a venda existe
            var vendaExists = await _context.Venda
                .ApplySedeFilter(access)
                .FirstOrDefaultAsync(v => v.Id == request.VendaId, cancellationToken);

            if (vendaExists == null)
                throw new NotFoundException("Venda não encontrada.");

            var responsavelVendaId = vendaExists.VendedorAtualId ?? vendaExists.VendedorId;

            if (responsavelVendaId != access.UsuarioId)
                throw new UnauthorizedException("Você só pode vincular chats às suas próprias vendas.");

            // 🔎 Verifica se já existe vínculo para esse chat/user
            var chatIdentifiers = new[] { request.WhatsappChatId, request.WhatsappChatNumero }
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList();

            var existingLink = await _context.VendaWhatsapp
                .Include(x => x.Venda)
                .FirstOrDefaultAsync(x =>
                    chatIdentifiers.Contains(x.WhatsappChatId) &&
                    x.WhatsappUserId == responsavelVendaId.ToString(),
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
                WhatsappUserId = responsavelVendaId.ToString()
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
