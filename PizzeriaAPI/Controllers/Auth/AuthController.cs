using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using PizzeriaAPI.DTOs.Auth;
using PizzeriaAPI.Services.Interfaces;

namespace PizzeriaAPI.Controllers.Auth
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;
        private readonly IValidator<LoginRequestDto> _validator;

        public AuthController(IAuthService authService,
                              ILogger<AuthController> logger,
                              IValidator<LoginRequestDto> validator)
        {
            _authService = authService;
            _logger = logger;
            _validator = validator;
        }

        // POST api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            if (request == null)
                return BadRequest(new { mensaje = "Datos de login requeridos" });

            var validacion = await _validator.ValidateAsync(request);
            if (!validacion.IsValid)
                return BadRequest(validacion.Errors.Select(e => e.ErrorMessage));

            var resultado = await _authService.LoginAsync(request);

            if (resultado == null)
            {
                _logger.LogWarning("Login fallido para email: {Email}", request.Email);
                return Unauthorized(new { mensaje = "Credenciales inválidas" });
            }

            _logger.LogInformation("Login exitoso: {Email}", resultado.Email);
            return Ok(resultado);
        }
    }
}