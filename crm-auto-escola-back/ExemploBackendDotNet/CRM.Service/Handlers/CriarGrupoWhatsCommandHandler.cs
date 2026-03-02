using Exemplo.Domain.Model;
using Exemplo.Persistence;
using Exemplo.Service.Commands;
using Exemplo.Service.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Exemplo.Service.Handlers
{
    public class CriarGrupoWhatsCommandHandler
        : IRequestHandler<CriarGrupoWhatsCommand, GrupoWhatsappModel>
    {
        private readonly ExemploDbContext _context;

        public CriarGrupoWhatsCommandHandler(ExemploDbContext context)
        {
            _context = context;
        }

        public async Task<GrupoWhatsappModel> Handle(
            CriarGrupoWhatsCommand request,
            CancellationToken cancellationToken)
        {
            // 🔎 Verifica se a venda existe
            var grupoExists = await _context.GrupoWhatsapp
                .FirstOrDefaultAsync(v => v.Nome == request.Nome, cancellationToken);

            if (grupoExists != null)
                throw new ConflictException("Grupo já criado.");

            var usuarioExiste = await _context.Usuario
                .AnyAsync(u => u.Id == request.UsuarioId, cancellationToken);

            if (!usuarioExiste)
                throw new NotFoundException("Usuário não encontrado.");

            var dataInicialDe = request.DataInicialDe?.Date;
            var dataInicialAte = request.DataInicialAte?.Date;

            if (dataInicialDe.HasValue && dataInicialAte.HasValue && dataInicialDe > dataInicialAte)
                throw new ValidationException("Data inicial não pode ser maior que a data final.");

            var grupo = new GrupoWhatsappModel()
            {
                Nome = request.Nome,
                UsuarioId = request.UsuarioId
            };

            var entity = _context.GrupoWhatsapp.Add(grupo);
            await _context.SaveChangesAsync(cancellationToken);

            if (request.Status.HasValue || request.ServicoId.HasValue || request.DataInicialDe.HasValue || request.DataInicialAte.HasValue)
            {
                var leadsQuery = _context.Venda
                    .Include(v => v.VendaWhatsapp)
                    .Where(v => v.VendedorId == request.UsuarioId)
                    .AsQueryable();

                if (request.Status.HasValue)
                    leadsQuery = leadsQuery.Where(v => v.Status == request.Status.Value);

                if (request.ServicoId.HasValue)
                    leadsQuery = leadsQuery.Where(v => v.ServicoId == request.ServicoId.Value);

                if (dataInicialDe.HasValue)
                {
                    var dataInicialDeUtc = DateTime.SpecifyKind(dataInicialDe.Value, DateTimeKind.Utc);
                    leadsQuery = leadsQuery.Where(v => v.DataInicial >= dataInicialDeUtc);
                }

                if (dataInicialAte.HasValue)
                {
                    var dataInicialAteUtc = DateTime.SpecifyKind(dataInicialAte.Value, DateTimeKind.Utc);
                    leadsQuery = leadsQuery.Where(v => v.DataInicial <= dataInicialAteUtc);
                }

                var leads = await leadsQuery.ToListAsync(cancellationToken);

                foreach (var lead in leads.Where(v => v.VendaWhatsapp == null))
                {
                    var whatsappChatId = BuildWhatsappChatId(lead.Contato);

                    if (string.IsNullOrWhiteSpace(whatsappChatId))
                        continue;

                    var vinculo = new VendaWhatsappModel
                    {
                        VendaId = lead.Id,
                        WhatsappChatId = whatsappChatId,
                        WhatsappUserId = request.UsuarioId.ToString()
                    };

                    _context.VendaWhatsapp.Add(vinculo);
                }

                await _context.SaveChangesAsync(cancellationToken);

                var leadIds = leads.Select(v => v.Id).ToList();

                var vendaWhatsIds = await _context.VendaWhatsapp
                    .Where(vw => leadIds.Contains(vw.VendaId))
                    .Select(vw => vw.Id)
                    .ToListAsync(cancellationToken);

                if (vendaWhatsIds.Count > 0)
                {
                    var gruposVenda = vendaWhatsIds.Select(id => new GrupoVendaWhatsappModel
                    {
                        IdGrupo = entity.Entity.Id,
                        IdVendaWhats = id
                    });

                    _context.GrupoVendaWhatsapp.AddRange(gruposVenda);
                    await _context.SaveChangesAsync(cancellationToken);
                }
            }

            return entity.Entity;
        }

        private static string BuildWhatsappChatId(string contato)
        {
            var digits = new string((contato ?? string.Empty).Where(char.IsDigit).ToArray());
            if (string.IsNullOrWhiteSpace(digits))
                return string.Empty;

            if (!digits.StartsWith("55", StringComparison.Ordinal))
                digits = $"55{digits}";

            return $"{digits}@c.us";
        }
    }
}
