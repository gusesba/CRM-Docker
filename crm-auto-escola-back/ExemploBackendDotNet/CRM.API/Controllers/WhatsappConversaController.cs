using Exemplo.Domain.Model;
using Exemplo.Service.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Renova.API.Controllers
{
    [ApiController]
    [Route("api/whatsapp/conversas")]
    [Authorize("UserOrAdmin")]
    public class WhatsappConversaController : ControllerBase
    {
        private readonly IMediator _mediator;

        public WhatsappConversaController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("usuario/{usuarioId}")]
        [ProducesResponseType(typeof(List<ChatWhatsappModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> BuscarConversasUsuario([FromRoute] int usuarioId)
        {
            var conversas = await _mediator.Send(new ListarConversasUsuarioQuery
            {
                UsuarioId = usuarioId
            });

            return Ok(conversas);
        }

        [HttpGet("{chatWhatsappId}/mensagens")]
        [ProducesResponseType(typeof(List<MensagemWhatsappModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> BuscarMensagensConversa([FromRoute] int chatWhatsappId)
        {
            var mensagens = await _mediator.Send(new ListarMensagensConversaQuery
            {
                ChatWhatsappId = chatWhatsappId
            });

            return Ok(mensagens);
        }
    }
}
