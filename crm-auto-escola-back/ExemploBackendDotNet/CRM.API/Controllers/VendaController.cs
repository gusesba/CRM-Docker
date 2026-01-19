using Exemplo.Domain.Model;
using Exemplo.Domain.Model.Dto;
using Exemplo.Domain.Settings;
using Exemplo.Service.Commands;
using Exemplo.Service.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Renova.API.Controllers
{
    [ApiController]
    [Route("api/venda")]
    [Authorize("UserOrAdmin")]
    public class VendaController : ControllerBase
    {
        private readonly IMediator _mediator;
        public VendaController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [ProducesResponseType(typeof(VendaModel), StatusCodes.Status201Created)]

        public async Task<IActionResult> Registrar([FromBody] CriarVendaCommand command)
        {
            var venda = await _mediator.Send(command);

            return Created($"/api/venda/{venda.Id}",venda);
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<VendaModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> BuscarVendas([FromQuery] BuscarVendasQuery query)
        {
            var vendas = await _mediator.Send(query);

            return Ok(vendas);
        }

        [HttpGet("{Id}")]
        [ProducesResponseType(typeof(VendaModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> BuscarVendaById([FromRoute] BuscarVendaByIdQuery query)
        {
            var venda = await _mediator.Send(query);

            return Ok(venda);
        }

        [HttpPut]
        [ProducesResponseType(typeof(VendaModel), StatusCodes.Status200OK)]

        public async Task<IActionResult> Editar([FromBody] EditarVendaCommand command)
        {
            var venda = await _mediator.Send(command);

            return Ok(venda);
        }

        [HttpPatch("transferir")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> TransferirVendas([FromBody] TransferirVendasCommand command)
        {
            await _mediator.Send(command);
            return NoContent();
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> Dashboard([FromQuery] DashboardQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("whatsapp")]
        public async Task<IActionResult> Whatsapp([FromQuery] GetVendaByWhatsappQuery query)
        {
            var result = await _mediator.Send(query);

            return Ok(result);
        }

        [HttpGet("whatsapp/vinculo/{vendaId}")]
        [ProducesResponseType(typeof(VendaChatVinculoDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> BuscarVinculoWhatsapp([FromRoute] GetVendaChatVinculoQuery query)
        {
            var result = await _mediator.Send(query);

            return Ok(result);
        }

        [HttpGet("whatsapp/grupos")]
        [ProducesResponseType(typeof(List<GrupoWhatsappDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> BuscarGruposWhatsapp([FromQuery] BuscarGruposWhatsappQuery query)
        {
            var grupos = await _mediator.Send(query);

            return Ok(grupos);
        }

        [HttpGet("whatsapp/grupos/venda/{vendaId}")]
        [ProducesResponseType(typeof(List<GrupoWhatsappDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> BuscarGruposWhatsappPorVenda([FromRoute] int vendaId)
        {
            var grupos = await _mediator.Send(new BuscarGruposWhatsappQuery { VendaId = vendaId });

            return Ok(grupos);
        }

        [HttpGet("whatsapp/grupos/chat/{whatsappChatId}")]
        [ProducesResponseType(typeof(List<GrupoWhatsappDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> BuscarGruposWhatsappPorChat([FromRoute] string whatsappChatId)
        {
            var grupos = await _mediator.Send(new BuscarGruposWhatsappQuery { WhatsappChatId = whatsappChatId });

            return Ok(grupos);
        }

        [HttpGet("whatsapp/vinculos")]
        [ProducesResponseType(typeof(List<VendaWhatsappDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> BuscarVinculosWhatsapp([FromQuery] ListarVendasWhatsappQuery query)
        {
            var vinculos = await _mediator.Send(query);

            return Ok(vinculos);
        }

        [HttpPost("vincular")]
        public async Task<IActionResult> VincularVendaWhats(
            [FromBody] VincularVendaWhatsCommand command,
            CancellationToken cancellationToken)
        {
            var vinculo = await _mediator.Send(command, cancellationToken);
            return Ok(vinculo);
        }

        [HttpPost("grupo")]
        public async Task<IActionResult> CriarGrupoWhats([FromBody] CriarGrupoWhatsCommand command, CancellationToken cancellationToken)
        {
            var grupo = await _mediator.Send(command, cancellationToken);
            return Ok(grupo);
        }

        [HttpPost("adicionargrupo")]
        public async Task<IActionResult> AdicionarAoGrupo([FromBody] AdicionarAoGrupoWhatsCommand command, CancellationToken cancellationToken)
        {
            var grupoVenda = await _mediator.Send(command,cancellationToken);
            return Ok(grupoVenda);
        }

        [HttpDelete("grupo/{grupoId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> ExcluirGrupoWhats([FromRoute] int grupoId, CancellationToken cancellationToken)
        {
            await _mediator.Send(new ExcluirGrupoWhatsCommand { GrupoId = grupoId }, cancellationToken);
            return NoContent();
        }

        [HttpDelete("grupo/conversas")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> RemoverConversasGrupoWhats(
            [FromBody] RemoverConversasGrupoWhatsCommand command,
            CancellationToken cancellationToken)
        {
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }

        [HttpDelete("whatsapp/{vendaWhatsappId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> ExcluirVendaWhatsapp([FromRoute] int vendaWhatsappId, CancellationToken cancellationToken)
        {
            await _mediator.Send(new ExcluirVendaWhatsappCommand { VendaWhatsappId = vendaWhatsappId }, cancellationToken);
            return NoContent();
        }
    }
}
