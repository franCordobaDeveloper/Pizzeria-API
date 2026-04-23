using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using PizzeriaAPI.DTOs.Clientes;
using PizzeriaAPI.Services.Interfaces;
using System.Runtime.InteropServices;

namespace PizzeriaAPI.Controllers.Clientes
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ClientesController : ControllerBase
    {
        private readonly IClienteService _clienteService;
        private readonly ILogger<ClientesController> _logger;
        private readonly IValidator<ClienteRequestDto> _validator;

        public ClientesController(IClienteService clienteService, ILogger<ClientesController> logger, IValidator<ClienteRequestDto> validator)
        {
            _clienteService = clienteService;
            _logger = logger;
            _validator = validator;
        }

        // POST api/clientes
        [HttpPost]
        public async Task<IActionResult> CrearCliente([FromBody] ClienteRequestDto clienteRequest)
        {
            if (clienteRequest == null)
            {
                return BadRequest(new { mensaje = "Los datos del cliente son requeridos." });
            }

            var validacion = await _validator.ValidateAsync(clienteRequest);
            if (!validacion.IsValid)
            {
                _logger.LogWarning("Validación fallida para crear cliente: {Errors}", validacion.Errors);
                return BadRequest(new { mensaje = "Datos del cliente no válidos.", errores = validacion.Errors });
            }

            var cliente = await _clienteService.CrearClienteAsync(clienteRequest);

            if (cliente == null)
            {
                _logger.LogError("Error al crear el cliente");
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Ocurrió un error al crear el cliente." });
            }

            _logger.LogInformation("Cliente creado exitosamente con ID: {Id}", cliente.Id);

            return Ok(cliente);
        }

        // GET api/clientes/{telefono}
        [HttpGet("telefono/{telefono}")]
        public async Task<IActionResult> BuscarClientePorTelefono(string telefono)
        {
            
            if (string.IsNullOrWhiteSpace(telefono))
            {
                _logger?.LogWarning("Intento de búsqueda con teléfono vacío");
                return BadRequest(new { mensaje = "El número de teléfono es requerido" });
            }

            var cliente = await _clienteService.BuscarClientePorTelefonoAsync(telefono);
            if (cliente == null)
            {
                _logger.LogInformation("Cliente no encontrado con teléfono: {Telefono}", telefono);
                return NotFound(new { mensaje = $"No se encontró un cliente con el teléfono {telefono}." });
            }
            return Ok(cliente);
        }

        // GET api/clientes/buscar?q=juana
        [HttpGet("buscar")]
        public async Task<IActionResult> BuscarClientes([FromQuery] string q)
        {
            if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            {
                return Ok(new List<ClienteResponseDto>()); // No buscar con menos de 2 caracteres
            }

            var clientes = await _clienteService.BuscarClientesAsync(q);
            return Ok(clientes);
        }
    }
}
