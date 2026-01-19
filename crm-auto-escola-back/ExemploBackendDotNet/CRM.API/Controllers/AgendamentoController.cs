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
    [Route("api/agendamento")]
    [Authorize("UserOrAdmin")]
    public class AgendamentoController : ControllerBase
    {
        private readonly IMediator _mediator;
        public AgendamentoController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [ProducesResponseType(typeof(AgendamentoModel), StatusCodes.Status201Created)]

        public async Task<IActionResult> Registrar([FromBody] CriarAgendamentoCommand command)
        {
            var agendamento = await _mediator.Send(command);

            return Created($"/api/agendamento/{agendamento.Id}",agendamento);
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<AgendamentoModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> BuscarAgendamentos([FromQuery] BuscarAgendamentosQuery query)
        {
            var agendamentos = await _mediator.Send(query);

            return Ok(agendamentos);
        }
    }
}
