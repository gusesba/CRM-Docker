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
    [Route("api/condicaoVenda")]
    [Authorize("UserOrAdmin")]
    public class CondicaoVendaController : ControllerBase
    {
        private readonly IMediator _mediator;
        public CondicaoVendaController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [Authorize("AdminOnly")]
        [ProducesResponseType(typeof(CondicaoVendaModel), StatusCodes.Status201Created)]

        public async Task<IActionResult> Registrar([FromBody] CriarCondicaoVendaCommand command)
        {
            var condicaoVenda = await _mediator.Send(command);

            return Created($"/api/condicaoVenda/{condicaoVenda.Id}",condicaoVenda);
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<CondicaoVendaModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> BuscarCondicaoVendas([FromQuery] BuscarCondicaoVendasQuery query)
        {
            var condicaoVendas = await _mediator.Send(query);

            return Ok(condicaoVendas);
        }
    }
}
