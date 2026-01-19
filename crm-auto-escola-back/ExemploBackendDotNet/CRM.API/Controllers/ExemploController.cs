using Exemplo.Domain.Model;
using Exemplo.Service.Commands;
using Exemplo.Service.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Renova.API.Controllers
{
    [ApiController]
    [Route("api/exemplo")]
    [Authorize]
    public class ExemploController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ExemploController(IMediator mediator) 
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ExemploModel),StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetExemplo([FromQuery] ExemploQuery request)
        {
            var exemplo = await _mediator.Send(request);

            return Ok(exemplo);
        }

        [HttpPost]
        [ProducesResponseType(typeof(ExemploModel), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> PostExemplo([FromBody] ExemploCommand command)
        {
            var exemplo = await _mediator.Send(command);

            return Created(String.Empty,exemplo);
        }
    }
}
