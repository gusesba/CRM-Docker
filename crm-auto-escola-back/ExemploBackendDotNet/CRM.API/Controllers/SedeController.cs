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
    [Route("api/sede")]
    [Authorize("UserOrAdmin")]
    public class SedeController : ControllerBase
    {
        private readonly IMediator _mediator;
        public SedeController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [Authorize("AdminOnly")]
        [ProducesResponseType(typeof(SedeModel), StatusCodes.Status201Created)]

        public async Task<IActionResult> Registrar([FromBody] CriarSedeCommand command)
        {
            var sede = await _mediator.Send(command);

            return Created($"/api/sede/{sede.Id}",sede);
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<SedeModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> BuscarSedes([FromQuery] BuscarSedesQuery query)
        {
            var sedes = await _mediator.Send(query);

            return Ok(sedes);
        }
    }
}
