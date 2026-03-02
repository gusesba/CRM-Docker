using Exemplo.Domain.Model;
using Exemplo.Domain.Model.Dto;
using Exemplo.Domain.Model.Enum;
using Exemplo.Persistence;
using Exemplo.Service.Queries;
using Exemplo.Service.Security;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Exemplo.Service.Handlers
{
    public class GetVendaByWhatsappQueryHandler : IRequestHandler<GetVendaByWhatsappQuery, ChatStatusDto>
    {
        private readonly ExemploDbContext _context;
        private readonly IUsuarioContextService _usuarioContextService;

        public GetVendaByWhatsappQueryHandler(
            ExemploDbContext context,
            IUsuarioContextService usuarioContextService)
        {
            _context = context;
            _usuarioContextService = usuarioContextService;
        }

        public async Task<ChatStatusDto> Handle(
            GetVendaByWhatsappQuery request,
            CancellationToken cancellationToken
        )
        {
            var access = await _usuarioContextService.GetUsuarioSedeAccessAsync(cancellationToken);
            var chatIdentifiers = BuildChatIdentifiers(request.WhatsappChatId, request.WhatsappChatNumero);

            var vinculada = await _context.VendaWhatsapp
            .Include(x => x.Venda)
            .FirstOrDefaultAsync(x =>
                chatIdentifiers.Contains(x.WhatsappChatId) &&
                x.WhatsappUserId == request.WhatsappUserId &&
                (access.AllowAll || !access.SedeId.HasValue || x.Venda.SedeId == access.SedeId.Value),
                cancellationToken);

            if (vinculada != null)
            {
                return new ChatStatusDto(){
                   Status = WhatsStatusEnum.Criado,
                   Venda = vinculada.Venda
                };
            }

            var phone = request.Contato;

            string Normalize(string input)
            {
                return new string(input.Where(char.IsDigit).ToArray());
            }

            string RemoveNinthDigitAfterDDD(string phone)
            {
                if (phone.Length >= 11 && phone[2] == '9')
                {
                    return phone.Remove(2, 1);
                }

                return phone;
            }


            var normalizedPhone = Normalize(phone);

            if (normalizedPhone.StartsWith("55"))
                normalizedPhone = normalizedPhone.Substring(2);

            var phoneWithout9 = RemoveNinthDigitAfterDDD(normalizedPhone);

            var vendasQuery = _context.Venda
                .AsNoTracking()
                .Where(v => v.Contato != null)
                .ApplySedeFilter(access);

            var vendas = await vendasQuery.ToListAsync(cancellationToken);


            var venda = vendas.FirstOrDefault(v =>
            {
                var contato = Normalize(v.Contato);

                var contatoWithout9 = RemoveNinthDigitAfterDDD(contato);

                return
                    contato == normalizedPhone ||
                    contato == phoneWithout9 ||
                    contatoWithout9 == normalizedPhone ||
                    contatoWithout9 == phoneWithout9;
            });

            if (venda == null)
                return new ChatStatusDto()
                {
                    Status = WhatsStatusEnum.NaoEncontrado,
                    Venda = null
                };

            return new ChatStatusDto()
            {
                Status = WhatsStatusEnum.NaoCriado,
                Venda = venda
            };
        }

        private static List<string> BuildChatIdentifiers(string? chatId, string? chatNumero)
        {
            var identifiers = new HashSet<string>(StringComparer.Ordinal);

            if (!string.IsNullOrWhiteSpace(chatId))
            {
                identifiers.Add(chatId);
            }

            if (!string.IsNullOrWhiteSpace(chatNumero))
            {
                identifiers.Add(chatNumero);

                var digits = new string(chatNumero.Where(char.IsDigit).ToArray());
                if (!string.IsNullOrWhiteSpace(digits))
                {
                    foreach (var variant in BuildPhoneVariants(digits))
                    {
                        identifiers.Add(variant);
                        identifiers.Add($"{variant}@c.us");
                    }
                }
            }

            return identifiers.ToList();
        }

        private static List<string> BuildPhoneVariants(string digits)
        {
            var variants = new HashSet<string>(StringComparer.Ordinal);

            void AddVariant(string? value)
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    variants.Add(value);
                }
            }

            var withoutCountry = digits.StartsWith("55", StringComparison.Ordinal) ? digits.Substring(2) : digits;

            string RemoveNinthDigitAfterDDD(string phone)
            {
                if (phone.Length >= 11 && phone[2] == '9')
                {
                    return phone.Remove(2, 1);
                }

                return phone;
            }

            string AddNinthDigitAfterDDD(string phone)
            {
                if (phone.Length == 10)
                {
                    return $"{phone.Substring(0, 2)}9{phone.Substring(2)}";
                }

                return phone;
            }

            AddVariant(withoutCountry);
            AddVariant(RemoveNinthDigitAfterDDD(withoutCountry));
            AddVariant(AddNinthDigitAfterDDD(withoutCountry));
            AddVariant($"55{withoutCountry}");
            AddVariant($"55{RemoveNinthDigitAfterDDD(withoutCountry)}");
            AddVariant($"55{AddNinthDigitAfterDDD(withoutCountry)}");

            return variants.ToList();
        }
    }
}
