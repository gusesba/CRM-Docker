using Exemplo.Domain.Model;
using Exemplo.Domain.Model.Dto;
using Exemplo.Persistence;
using Exemplo.Service.Commands;
using Exemplo.Service.Exceptions;
using Exemplo.Service.Queries;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Exemplo.Service.Handlers
{
    public class SignUpCommandHandler : IRequestHandler<SignUpCommand, LoginDto>
    {
        private readonly ExemploDbContext _context;
        private readonly IMediator _mediator;
        public SignUpCommandHandler(ExemploDbContext context, IMediator mediator)
        {
            _context = context;
            _mediator = mediator;
        }

        public async Task<LoginDto> Handle(SignUpCommand request, CancellationToken cancellationToken)
        {
            var usuarioExistente = await _context.Usuario
                .FirstOrDefaultAsync(u => u.Usuario == request.Usuario);

            if (usuarioExistente != null)
                throw new ConflictException("Usuário já cadastrado.");

            if (!request.IsAdmin && !request.SedeId.HasValue)
                throw new ValidationException("Sede é obrigatória para usuários não administradores.");

            if (request.SedeId.HasValue)
            {
                var sedeExiste = await _context.Sede
                    .AnyAsync(s => s.Id == request.SedeId.Value, cancellationToken);

                if (!sedeExiste)
                    throw new NotFoundException("Sede não encontrada.");
            }

            string senhaHash = BCrypt.Net.BCrypt.HashPassword(request.Senha);

            var novoUsuario = new UsuarioModel()
            {
                Nome = request.Nome,
                Usuario = request.Usuario,
                SenhaHash = senhaHash,
                IsAdmin = request.IsAdmin,
                SedeId = request.SedeId
            };

            await _context.Usuario.AddAsync(novoUsuario);
            await _context.SaveChangesAsync();

            return await _mediator.Send(new LoginQuery()
            {
                Usuario = request.Usuario,
                Senha = request.Senha
            });
        }
    }
}
