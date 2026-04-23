using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PizzeriaAPI.Data;
using PizzeriaAPI.DTOs.Auth;
using PizzeriaAPI.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PizzeriaAPI.Services
{
    public class AuthService: IAuthService
    {
        private readonly PizzeriaContext _pizzeriaContext;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        // Constructor e Injeccion de dependencias
        public AuthService(PizzeriaContext pizzeriaContext, IConfiguration configuration, ILogger<AuthService> logger)
        {
            _pizzeriaContext = pizzeriaContext;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                _logger.LogWarning("Intento de login con email o password vacios");
                return null;
            }

            try
            {
                // Primero debo buscar al usuario por su email
                var emailNormalizado = request.Email.Trim().ToLower();
                var usuario = await _pizzeriaContext.Usuarios
                    .FirstOrDefaultAsync(u => u.Email == emailNormalizado && u.Activo);

                // debo verificar que el usuario exista y que la contraseña sea correcta de lo contrario retorno null
                if (usuario == null || !BCrypt.Net.BCrypt.Verify(request.Password, usuario.PasswordHash) )
                {
                    _logger.LogWarning("Intento de login fallido para email: {Email}", request.Email);
                }

                // Generar el token JWT
                var token = GenerarToken(usuario!.Id, usuario.Email, usuario.Rol);

                _logger.LogInformation("Usuario con email: {Email} logueado exitosamente", request.Email);
            
                return new LoginResponseDto
                {
                    Id = usuario.Id,
                    Nombre = usuario.Nombre,
                    Email = usuario.Email,
                    Rol = usuario.Rol,
                    Token = token
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al intentar loguear al usuario con email: {Email}", request.Email);
                return null;
            }
        }

        private string GenerarToken(int id, string email, string rol)
        {
            // hay que validar la configuracion del JWT
            var jwtKey = _configuration["Jwt:Key"];
            var jwtIssuer = _configuration["Jwt:Issuer"];
            var jwtAudience = _configuration["Jwt:Audience"];
            var jwtExpiresInHours = _configuration["Jwt:ExpireInHours"];

            if (string.IsNullOrEmpty(jwtKey))
                throw new InvalidOperationException("Jwt:Key no está configurada en appsettings.json");

            if (jwtKey.Length < 32)
                throw new InvalidOperationException("Jwt:Key debe tener al menos 32 caracteres");

            if (string.IsNullOrEmpty(jwtIssuer))
                throw new InvalidOperationException("Jwt:Issuer no está configurada");

            if (string.IsNullOrEmpty(jwtAudience))
                throw new InvalidOperationException("Jwt:Audience no está configurada");

            if (!double.TryParse(jwtExpiresInHours, out var expiresInHours))
                expiresInHours = 8; // Valor por defecto: 8 horas
            else if (expiresInHours <= 0)
                expiresInHours = 8;

            // Claims del token
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim(ClaimTypes.Role, rol)
            };

            // clade de firma del token

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credenciales = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // genero el token

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(expiresInHours),
                signingCredentials: credenciales
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
