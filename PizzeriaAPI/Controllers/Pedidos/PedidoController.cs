using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PizzeriaAPI.DTOs.Pedidos;
using PizzeriaAPI.Services.Interfaces;
using System.Security.Claims;


namespace PizzeriaAPI.Controllers.Pedidos
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class PedidoController : ControllerBase
    {
        private readonly IPedidoService _pedidoService;
        private readonly ILogger<PedidoController> _logger;
        private readonly IValidator<PedidoRequestDto> _validator;

        public PedidoController(
            IPedidoService pedidoService,
            ILogger<PedidoController> logger,
            IValidator<PedidoRequestDto> validator)
        {
            _pedidoService = pedidoService;
            _logger = logger;
            _validator = validator;
        }

        // POST: api/Pedido
        [HttpPost]
        public async Task<IActionResult> CrearPedido([FromBody] PedidoRequestDto request)
        {
            if (request == null) 
                   return BadRequest(new { Message = "El cuerpo de la solicitud no puede estar vacío." });
        
            var validacion = await _validator.ValidateAsync(request);
            if (!validacion.IsValid)
            {
                var errores = validacion.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
                return BadRequest(new { Message = "La solicitud contiene errores de validación.", Errors = errores });
            }

            var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            if( usuarioId == 0) 
                return Unauthorized(new { Message = "No se pudo identificar al usuario autenticado." });

            var pedido = await _pedidoService.CrearPedidoAsync(request, usuarioId);
            return CreatedAtAction(nameof(ObtenerPedidoPorId), new { id = pedido.Id }, pedido);
        }

        // GET : api/Pedido/activos
        [HttpGet("activos")]
        public async Task<IActionResult> ObtenerPedidosActivos()
        {
            var pedidos = await _pedidoService.ObtenerPedidosActivosAsync();
            return Ok(pedidos);
        }

        // GET api/pedido/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerPedidoPorId(int id)
        {
            if (id <= 0)
                return BadRequest(new { mensaje = "ID inválido" });

            var pedido = await _pedidoService.ObtenerPedidoPorIdAsync(id);
            if (pedido == null)
                return NotFound(new { mensaje = $"No se encontró el pedido con ID {id}" });

            return Ok(pedido);
        }

        // PUT api/pedido/{id}/cancelar
        [HttpPut("{id}/cancelar")]
        [Authorize]  // ← permitir tanto admin como cajero
        public async Task<IActionResult> CancelarPedido(int id)
        {
            if (id <= 0)
                return BadRequest(new { mensaje = "ID inválido" });

            var cancelado = await _pedidoService.CancelarPedidoAsync(id);
            if (!cancelado)
                return NotFound(new { mensaje = $"No se encontró un pedido activo con ID {id}" });

            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarPedido(int id, [FromBody] PedidoRequestDto request)
        {
            if (request == null)
                return BadRequest(new { mensaje = "Los datos del pedido son requeridos" });

            var validacion = await _validator.ValidateAsync(request);
            if (!validacion.IsValid)
                return BadRequest(new { errores = validacion.Errors.Select(e => e.ErrorMessage) });

            var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (usuarioId == 0)
                return Unauthorized(new { mensaje = "Usuario no autenticado" });

            var pedido = await _pedidoService.ActualizarPedidoAsync(id, request, usuarioId);
            if (pedido == null)
                return NotFound(new { mensaje = $"No se encontró un pedido activo con ID {id}" });

            return Ok(pedido);
        }
    }
}
