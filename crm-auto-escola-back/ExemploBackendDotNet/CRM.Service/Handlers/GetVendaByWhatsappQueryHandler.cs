using Exemplo.Domain.Model;
using Exemplo.Domain.Model.Dto;
using Exemplo.Domain.Model.Enum;
using Exemplo.Persistence;
using Exemplo.Service.Queries;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Exemplo.Service.Handlers
{
    public class GetVendaByWhatsappQueryHandler : IRequestHandler<GetVendaByWhatsappQuery, ChatStatusDto>
    {
        private readonly ExemploDbContext _context;

        public GetVendaByWhatsappQueryHandler(ExemploDbContext context)
        {
            _context = context;
        }

        public async Task<ChatStatusDto> Handle(
            GetVendaByWhatsappQuery request,
            CancellationToken cancellationToken
        )
        {
            var vinculada = await _context.VendaWhatsapp
            .Include(x => x.Venda)
            .FirstOrDefaultAsync(x =>
                x.WhatsappChatId == request.WhatsappChatId &&
                x.WhatsappUserId == request.WhatsappUserId,
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
                // precisa ter pelo menos DDD + 9 + número
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

            var vendas = await _context.Venda
                .AsNoTracking()
                .Where(v => v.Contato != null)
                .ToListAsync(cancellationToken);


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

    }
}
