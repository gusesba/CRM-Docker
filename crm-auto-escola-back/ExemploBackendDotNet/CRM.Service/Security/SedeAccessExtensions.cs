using Exemplo.Domain.Model;
using Exemplo.Service.Exceptions;

namespace Exemplo.Service.Security
{
    public static class SedeAccessExtensions
    {
        public static IQueryable<SedeModel> ApplySedeFilter(
            this IQueryable<SedeModel> query,
            UsuarioSedeAccess access)
        {
            if (!access.AllowAll && access.SedeId.HasValue)
            {
                query = query.Where(s => s.Id == access.SedeId.Value);
            }

            return query;
        }

        public static IQueryable<UsuarioModel> ApplySedeFilter(
            this IQueryable<UsuarioModel> query,
            UsuarioSedeAccess access)
        {
            if (!access.AllowAll && access.SedeId.HasValue)
            {
                query = query.Where(u => u.SedeId == access.SedeId.Value);
            }

            return query;
        }

        public static IQueryable<VendaModel> ApplySedeFilter(
            this IQueryable<VendaModel> query,
            UsuarioSedeAccess access)
        {
            if (!access.AllowAll && access.SedeId.HasValue)
            {
                query = query.Where(v => v.SedeId == access.SedeId.Value);
            }

            return query;
        }

        public static IQueryable<AgendamentoModel> ApplySedeFilter(
            this IQueryable<AgendamentoModel> query,
            UsuarioSedeAccess access)
        {
            if (!access.AllowAll && access.SedeId.HasValue)
            {
                query = query.Where(a => a.Venda.SedeId == access.SedeId.Value);
            }

            return query;
        }

        public static IQueryable<VendaWhatsappModel> ApplySedeFilter(
            this IQueryable<VendaWhatsappModel> query,
            UsuarioSedeAccess access)
        {
            if (!access.AllowAll && access.SedeId.HasValue)
            {
                query = query.Where(vw => vw.Venda.SedeId == access.SedeId.Value);
            }

            return query;
        }

        public static IQueryable<GrupoWhatsappModel> ApplySedeFilter(
            this IQueryable<GrupoWhatsappModel> query,
            UsuarioSedeAccess access)
        {
            if (!access.AllowAll && access.SedeId.HasValue)
            {
                query = query.Where(g => g.Usuario.SedeId == access.SedeId.Value);
            }

            return query;
        }

        public static IQueryable<ChatWhatsappModel> ApplySedeFilter(
            this IQueryable<ChatWhatsappModel> query,
            UsuarioSedeAccess access)
        {
            if (!access.AllowAll && access.SedeId.HasValue)
            {
                query = query.Where(c => c.Usuario.SedeId == access.SedeId.Value);
            }

            return query;
        }

        public static IQueryable<MensagemWhatsappModel> ApplySedeFilter(
            this IQueryable<MensagemWhatsappModel> query,
            UsuarioSedeAccess access)
        {
            if (!access.AllowAll && access.SedeId.HasValue)
            {
                query = query.Where(m => m.ChatWhatsapp.Usuario.SedeId == access.SedeId.Value);
            }

            return query;
        }

        public static void EnsureSameSede(
            this UsuarioSedeAccess access,
            int? sedeId,
            string message)
        {
            if (access.AllowAll || !access.SedeId.HasValue)
            {
                return;
            }

            if (!sedeId.HasValue || sedeId.Value != access.SedeId.Value)
            {
                throw new UnauthorizedException(message);
            }
        }
    }
}
