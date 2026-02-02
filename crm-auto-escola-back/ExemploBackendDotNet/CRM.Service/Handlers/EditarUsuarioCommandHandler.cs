using Exemplo.Domain.Model.Dto;
using Exemplo.Persistence;
using Exemplo.Service.Commands;
using Exemplo.Service.Exceptions;
using Exemplo.Service.Security;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Exemplo.Service.Handlers
{
    public class EditarUsuarioCommandHandler : IRequestHandler<EditarUsuarioCommand, UsuarioDto>
    {
        private readonly ExemploDbContext _context;
        private readonly IUsuarioContextService _usuarioContextService;

        public EditarUsuarioCommandHandler(
            ExemploDbContext context,
            IUsuarioContextService usuarioContextService)
        {
            _context = context;
            _usuarioContextService = usuarioContextService;
        }

        public async Task<UsuarioDto> Handle(EditarUsuarioCommand request, CancellationToken cancellationToken)
        {
            var access = await _usuarioContextService.GetUsuarioSedeAccessAsync(cancellationToken);
            var usuario = await _context.Usuario
                .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

            if (usuario == null)
                throw new NotFoundException("Usuário não encontrado.");

            access.EnsureSameSede(usuario.SedeId, "Usuário não pertence à sua sede.");

            var usuarioExistente = await _context.Usuario
                .AnyAsync(u => u.Usuario == request.Usuario && u.Id != request.Id, cancellationToken);

            if (usuarioExistente)
                throw new ConflictException("Usuário já cadastrado.");

            if (!request.IsAdmin && !request.SedeId.HasValue)
                throw new ValidationException("Sede é obrigatória para usuários não administradores.");

            if (!access.AllowAll && access.SedeId.HasValue)
            {
                access.EnsureSameSede(request.SedeId, "Não é permitido alterar o usuário para outra sede.");
            }

            if (request.SedeId.HasValue)
            {
                var sedeExiste = await _context.Sede
                    .AnyAsync(s => s.Id == request.SedeId.Value, cancellationToken);

                if (!sedeExiste)
                    throw new NotFoundException("Sede não encontrada.");
            }

            usuario.Nome = request.Nome;
            usuario.Usuario = request.Usuario;
            usuario.IsAdmin = request.IsAdmin;
            usuario.Status = request.Status;
            usuario.SedeId = request.SedeId;

            if (!string.IsNullOrWhiteSpace(request.Senha))
            {
                usuario.SenhaHash = BCrypt.Net.BCrypt.HashPassword(request.Senha);
            }

            await _context.SaveChangesAsync(cancellationToken);

            return new UsuarioDto
            {
                Id = usuario.Id,
                Nome = usuario.Nome,
                Usuario = usuario.Usuario,
                Status = usuario.Status,
                SedeId = usuario.SedeId
            };
        }
    }
}
