using Exemplo.Domain.Model;
using Exemplo.Domain.Model.Dto;
using Exemplo.Persistence;
using Exemplo.Service.Commands;
using Exemplo.Service.Security;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Exemplo.Service.Handlers
{
    public class VincularConversasNumeroCommandHandler
        : IRequestHandler<VincularConversasNumeroCommand, ConversaVinculoResumoDto>
    {
        private readonly ExemploDbContext _context;
        private readonly IUsuarioContextService _usuarioContextService;

        public VincularConversasNumeroCommandHandler(
            ExemploDbContext context,
            IUsuarioContextService usuarioContextService)
        {
            _context = context;
            _usuarioContextService = usuarioContextService;
        }

        public async Task<ConversaVinculoResumoDto> Handle(
            VincularConversasNumeroCommand request,
            CancellationToken cancellationToken)
        {
            var access = await _usuarioContextService.GetUsuarioSedeAccessAsync(cancellationToken);

            if (request.Conversas == null || request.Conversas.Count == 0)
            {
                return new ConversaVinculoResumoDto();
            }

            var uniqueChats = request.Conversas
                .Where(c => !string.IsNullOrWhiteSpace(c.WhatsappChatId))
                .GroupBy(c => c.WhatsappChatId)
                .Select(g => g.First())
                .ToList();

            var chatIds = uniqueChats.Select(c => c.WhatsappChatId).ToList();

            var conversas = await _context.ChatWhatsapp
                .Include(c => c.Usuario)
                .Where(c => chatIds.Contains(c.WhatsappChatId))
                .ApplySedeFilter(access)
                .ToListAsync(cancellationToken);

            var conversasByChatId = conversas
                .GroupBy(c => c.WhatsappChatId)
                .ToDictionary(g => g.Key, g => g.First());

            var existingLinks = await _context.VendaWhatsapp
                .Include(vw => vw.Venda)
                .Where(vw => chatIds.Contains(vw.WhatsappChatId))
                .ApplySedeFilter(access)
                .ToListAsync(cancellationToken);

            var existingLinksByChatId = existingLinks
                .GroupBy(vw => vw.WhatsappChatId)
                .ToDictionary(g => g.Key, g => g.First());

            var leadsQuery = _context.Venda
                .Include(v => v.VendaWhatsapp)
                .ApplySedeFilter(access)
                .Where(v => v.Contato != null);

            var leads = await leadsQuery.ToListAsync(cancellationToken);

            var resumo = new ConversaVinculoResumoDto();

            foreach (var item in uniqueChats)
            {
                if (!conversasByChatId.TryGetValue(item.WhatsappChatId, out var conversa))
                {
                    resumo.ConversasSemLeadEncontrado++;
                    continue;
                }

                if (existingLinksByChatId.TryGetValue(item.WhatsappChatId, out var existingLink))
                {
                    resumo.ConversasJaVinculadas++;
                    continue;
                }

                var normalizedNumero = NormalizeDigits(item.Numero);
                if (string.IsNullOrWhiteSpace(normalizedNumero))
                {
                    resumo.ConversasSemLeadEncontrado++;
                    continue;
                }

                var candidateNumbers = BuildCandidates(normalizedNumero);
                var venda = leads.FirstOrDefault(v => MatchesContato(v.Contato, candidateNumbers));

                if (venda == null)
                {
                    resumo.ConversasSemLeadEncontrado++;
                    continue;
                }

                if (venda.VendaWhatsapp != null)
                {
                    resumo.ConversasJaVinculadas++;
                    continue;
                }

                var vinculo = new VendaWhatsappModel
                {
                    VendaId = venda.Id,
                    WhatsappChatId = item.WhatsappChatId,
                    WhatsappUserId = conversa.UsuarioId.ToString()
                };

                _context.VendaWhatsapp.Add(vinculo);
                await _context.SaveChangesAsync(cancellationToken);

                resumo.ConversasVinculadas++;
            }

            return resumo;
        }

        private static string NormalizeDigits(string input)
        {
            return new string(input.Where(char.IsDigit).ToArray());
        }

        private static string RemoveNinthDigitAfterDDD(string phone)
        {
            if (phone.Length >= 11 && phone[2] == '9')
            {
                return phone.Remove(2, 1);
            }

            return phone;
        }

        private static List<string> BuildCandidates(string phone)
        {
            var candidates = new List<string>();

            string AddCountryCode(string value)
            {
                return value.StartsWith("55") ? value : $"55{value}";
            }

            var normalized = phone;
            var withoutCountry = normalized.StartsWith("55") ? normalized.Substring(2) : normalized;
            var withoutNine = RemoveNinthDigitAfterDDD(withoutCountry);
            var withNine = withoutCountry.Length == 10
                ? $"{withoutCountry.Substring(0, 2)}9{withoutCountry.Substring(2)}"
                : withoutCountry;

            void AddCandidate(string value)
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    return;
                }

                if (!candidates.Contains(value))
                {
                    candidates.Add(value);
                }
            }

            AddCandidate(withoutCountry);
            AddCandidate(withoutNine);
            AddCandidate(withNine);
            AddCandidate(AddCountryCode(withoutCountry));
            AddCandidate(AddCountryCode(withoutNine));
            AddCandidate(AddCountryCode(withNine));

            return candidates;
        }

        private static bool MatchesContato(string? contato, List<string> candidates)
        {
            if (string.IsNullOrWhiteSpace(contato))
            {
                return false;
            }

            var normalized = NormalizeDigits(contato);
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return false;
            }

            var withoutCountry = normalized.StartsWith("55") ? normalized.Substring(2) : normalized;
            var withoutNine = RemoveNinthDigitAfterDDD(withoutCountry);
            var withCountry = normalized.StartsWith("55") ? normalized : $"55{normalized}";

            return candidates.Contains(normalized) ||
                   candidates.Contains(withoutCountry) ||
                   candidates.Contains(withoutNine) ||
                   candidates.Contains(withCountry);
        }
    }
}
