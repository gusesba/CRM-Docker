using Exemplo.Domain.Model;
using Exemplo.Domain.Settings;
using Exemplo.Service.Commands;
using Exemplo.Service.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Renova.API.Controllers
{
    [ApiController]
    [Route("api/servico")]
    [Authorize("UserOrAdmin")]
    public class ServicoController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ServicoController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [Authorize("AdminOnly")]
        [ProducesResponseType(typeof(ServicoModel), StatusCodes.Status201Created)]

        public async Task<IActionResult> Registrar([FromBody] CriarServicoCommand command)
        {
            var servico = await _mediator.Send(command);

            return Created($"/api/servico/{servico.Id}",servico);
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<ServicoModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> BuscarServicos([FromQuery] BuscarServicosQuery query)
        {
            var servicos = await _mediator.Send(query);

            return Ok(servicos);
        }
    }
}
