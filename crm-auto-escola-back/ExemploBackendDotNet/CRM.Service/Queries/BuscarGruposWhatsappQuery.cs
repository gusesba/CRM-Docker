using Exemplo.Domain.Model.Dto;
using MediatR;

namespace Exemplo.Service.Queries
{
        public class BuscarGruposWhatsappQuery : IRequest<List<GrupoWhatsappDto>>
        {
            public int? Id { get; set; }
            public int? UsuarioId { get; set; }
            public int? VendaId { get; set; }
            public string? WhatsappChatId { get; set; }
        }
}
