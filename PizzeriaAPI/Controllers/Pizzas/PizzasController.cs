using Azure.Core;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PizzeriaAPI.Data;
using PizzeriaAPI.DTOs.Pizzas;
using PizzeriaAPI.Services.Interfaces;

namespace PizzeriaAPI.Controllers.Pizzas
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PizzasController : ControllerBase
    {
        private readonly IPizzaService _pizzaService;
        private readonly ILogger<PizzasController> _logger;
        private readonly IValidator<PizzaRequestDto> _validator;

        public PizzasController(IPizzaService pizzaService,
                        ILogger<PizzasController> logger,
                        IValidator<PizzaRequestDto> validator)
        {
            _pizzaService = pizzaService;
            _logger = logger;
            _validator = validator;
        }

        // GET api/pizzas - cajero y admin pueden usar esto
        [HttpGet()]
        public async Task<IActionResult> ObtenerPizzas()
        {
            var pizzas = await _pizzaService.ObtenerTodasLasPizzasAsync();
            return Ok(pizzas);
        }

        // GET api/pizzas/5 — cajero y admin
        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerPizzaPorId(int id) 
        {
            if (id <= 0)
            {
                _logger.LogWarning("Se intentó buscar pizza con ID inválido: {Id}", id);
                return BadRequest(new { mensaje = "El ID debe ser un número positivo" });
            }

            var pizza = await _pizzaService.ObtenerPizzaPorIdAsync(id);

            if (pizza == null)
            {
                _logger.LogWarning("Pizza con ID {Id} no encontrada", id);
                return NotFound(new { mensaje = $"No se encontró la pizza con ID {id}" });
            }

            return Ok(pizza);
        }

        // POST api/pizzas — solo admin
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> CrearPizza([FromBody] PizzaRequestDto pizzaRequestDto) 
        {
            if (pizzaRequestDto == null)
                return BadRequest(new { mensaje = "Datos de la pizza son requeridos." });

            var validacion = await _validator.ValidateAsync(pizzaRequestDto);
            if (!validacion.IsValid)
                return BadRequest(validacion.Errors.Select(e => e.ErrorMessage));

            var pizza = await _pizzaService.CrearPizzaAsync(pizzaRequestDto);
            
            if( pizza == null)
            {
                _logger.LogError("Error al crear la pizza");
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Error al crear la pizza" });
            }

            _logger.LogInformation("Pizza creada exitosamente con id {IdPizza}", pizza.Id);

            return CreatedAtAction(
                nameof(ObtenerPizzaPorId),
                new { id = pizza.Id },
                pizza
            );
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> EliminarPizzaPorIdAsync(int id)
        {
            var eliminado = await _pizzaService.EliminarPizzaPorIdAsync(id);

            if (!eliminado)
            {
                _logger.LogWarning("Intento de eliminación de pizza con ID {Id} que no existe", id);
                return NotFound(new { mensaje = "No se encontró la pizza a eliminar" });
            }

            _logger.LogInformation("Pizza con ID {Id} eliminada exitosamente", id);
            return NoContent();
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> ActualizarPizza(int id, [FromBody] PizzaRequestDto pizzaRequestDto)
        {
            if (pizzaRequestDto == null)
                return BadRequest(new { mensaje = "Datos de pizza requeridos" });
            var validacion = await _validator.ValidateAsync(pizzaRequestDto);
            if (!validacion.IsValid)
                return BadRequest(validacion.Errors.Select(e => e.ErrorMessage));
            var pizzaActualizada = await _pizzaService.ActualizarPizzaPorIdAsync(id, pizzaRequestDto);
            if (pizzaActualizada == null)
            {
                _logger.LogWarning("Intento de actualización de pizza con id {IdPizza} que no existe", id);
                return NotFound(new { mensaje = "No se encontró la pizza a actualizar" });
            }
            _logger.LogInformation("Pizza con id {IdPizza} actualizada exitosamente", id);
            return Ok(pizzaActualizada);

        }   
    }
}
