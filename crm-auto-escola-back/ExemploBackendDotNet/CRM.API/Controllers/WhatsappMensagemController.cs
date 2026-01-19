using Exemplo.Domain.Model;
using Exemplo.Service.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Renova.API.Controllers
{
    [ApiController]
    [Route("api/whatsapp/mensagens")]
    [Authorize("UserOrAdmin")]
    public class WhatsappMensagemController : ControllerBase
    {
        private readonly IMediator _mediator;

        public WhatsappMensagemController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(typeof(MensagemWhatsappModel), StatusCodes.Status201Created)]
        public async Task<IActionResult> RegistrarMensagem([FromBody] RegistrarMensagemWhatsappCommand command)
        {
            var mensagem = await _mediator.Send(command);

            return Created(string.Empty, mensagem);
        }
    }
}
