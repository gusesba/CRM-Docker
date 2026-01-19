using Exemplo.Domain.Model;
using Exemplo.Domain.Model.Dto;
using Exemplo.Domain.Settings;
using Exemplo.Service.Commands;
using Exemplo.Service.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Renova.API.Controllers
{
    [ApiController]
    [Route("api/usuario")]
    [Authorize("UserOrAdmin")]
    public class UsuarioController : ControllerBase
    {
        private readonly IMediator _mediator;
        public UsuarioController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(LoginDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Login([FromBody] LoginQuery request)
        {
            var token = await _mediator.Send(request);
            return Ok(token);
        }

        [HttpPost("registrar")]
        [Authorize("AdminOnly")]
        [ProducesResponseType(typeof(LoginDto), StatusCodes.Status201Created)]
        public async Task<IActionResult> Registrar([FromBody] SignUpCommand command)
        {
            var token = await _mediator.Send(command);

            return Created("",token);
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<UsuarioDto>), StatusCodes.Status200OK)]

        public async Task<IActionResult> BuscarUsuarios([FromQuery] BuscarUsuariosQuery query)
        {
            var usuarios = await _mediator.Send(query);

            return Ok(usuarios);
        }

        [HttpPut]
        [Authorize("AdminOnly")]
        [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Editar([FromBody] EditarUsuarioCommand command)
        {
            var usuario = await _mediator.Send(command);

            return Ok(usuario);
        }

        [HttpGet("validartoken")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult ValidarToken()
        {
            return Ok(new { valido = true });
        }
    }
}
