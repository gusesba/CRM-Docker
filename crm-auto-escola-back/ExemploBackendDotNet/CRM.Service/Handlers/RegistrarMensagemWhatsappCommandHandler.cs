using Exemplo.Domain.Model;
using Exemplo.Persistence;
using Exemplo.Service.Commands;
using Exemplo.Service.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Exemplo.Service.Handlers
{
    public class RegistrarMensagemWhatsappCommandHandler
        : IRequestHandler<RegistrarMensagemWhatsappCommand, MensagemWhatsappModel>
    {
        private readonly ExemploDbContext _context;

        public RegistrarMensagemWhatsappCommandHandler(ExemploDbContext context)
        {
            _context = context;
        }

        public async Task<MensagemWhatsappModel> Handle(
            RegistrarMensagemWhatsappCommand request,
            CancellationToken cancellationToken)
        {
            var usuarioExiste = await _context.Usuario
                .AnyAsync(u => u.Id == request.UserId, cancellationToken);

            if (!usuarioExiste)
                throw new NotFoundException("Usuário não encontrado.");

            var chat = await _context.ChatWhatsapp
                .FirstOrDefaultAsync(
                    c => c.UsuarioId == request.UserId && c.WhatsappChatId == request.ChatId,
                    cancellationToken);

            if (chat == null)
            {
                chat = new ChatWhatsappModel
                {
                    UsuarioId = request.UserId,
                    WhatsappChatId = request.ChatId,
                    NomeChat = request.ChatName
                };

                _context.ChatWhatsapp.Add(chat);
                await _context.SaveChangesAsync(cancellationToken);
            }
            else if (!string.IsNullOrWhiteSpace(request.ChatName) &&
                     !string.Equals(chat.NomeChat, request.ChatName, StringComparison.Ordinal))
            {
                chat.NomeChat = request.ChatName;
                _context.ChatWhatsapp.Update(chat);
                await _context.SaveChangesAsync(cancellationToken);
            }

            var mensagemExistente = await _context.MensagemWhatsapp
                .FirstOrDefaultAsync(
                    m => m.ChatWhatsappId == chat.Id && m.MensagemId == request.Message.Id,
                    cancellationToken);

            if (mensagemExistente != null)
                return mensagemExistente;

            var mensagem = new MensagemWhatsappModel
            {
                ChatWhatsappId = chat.Id,
                MensagemId = request.Message.Id,
                Body = request.Message.Body,
                FromMe = request.Message.FromMe,
                Timestamp = request.Message.Timestamp,
                Type = request.Message.Type,
                HasMedia = request.Message.HasMedia,
                MediaUrl = request.Message.MediaUrl
            };

            _context.MensagemWhatsapp.Add(mensagem);
            await _context.SaveChangesAsync(cancellationToken);

            return mensagem;
        }
    }
}
