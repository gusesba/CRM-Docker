using Exemplo.Domain.Model;
using Exemplo.Persistence;
using Exemplo.Service.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Exemplo.Service.Security
{
    public interface IUsuarioContextService
    {
        Task<UsuarioModel> GetUsuarioAsync(CancellationToken cancellationToken);
        Task<UsuarioSedeAccess> GetUsuarioSedeAccessAsync(CancellationToken cancellationToken);
    }

    public sealed record UsuarioSedeAccess(int UsuarioId, int? SedeId, bool AllowAll, bool IsAdmin);

    public class UsuarioContextService : IUsuarioContextService
    {
        private readonly ExemploDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UsuarioContextService(
            ExemploDbContext context,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<UsuarioModel> GetUsuarioAsync(CancellationToken cancellationToken)
        {
            var userIdValue = _httpContextAccessor.HttpContext?.User?.FindFirst("UserId")?.Value;
            if (!int.TryParse(userIdValue, out var userId))
                throw new UnauthorizedException("Usuário não autenticado.");

            var usuario = await _context.Usuario
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

            if (usuario == null)
                throw new UnauthorizedException("Usuário não autenticado.");

            return usuario;
        }

        public async Task<UsuarioSedeAccess> GetUsuarioSedeAccessAsync(CancellationToken cancellationToken)
        {
            var usuario = await GetUsuarioAsync(cancellationToken);

            if (!usuario.SedeId.HasValue && !usuario.IsAdmin)
                throw new UnauthorizedException("Usuário sem sede cadastrada.");

            var allowAll = usuario.IsAdmin && !usuario.SedeId.HasValue;

            return new UsuarioSedeAccess(usuario.Id, usuario.SedeId, allowAll, usuario.IsAdmin);
        }
    }
}
