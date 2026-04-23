using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PizzeriaAPI.Data;
using PizzeriaAPI.DTOs.Auth;
using PizzeriaAPI.DTOs.Clientes;
using PizzeriaAPI.DTOs.Gastos;
using PizzeriaAPI.DTOs.Pedidos;
using PizzeriaAPI.DTOs.Pizzas;
using PizzeriaAPI.Services;
using PizzeriaAPI.Services.Interfaces;
using PizzeriaAPI.Validators.Auth;
using PizzeriaAPI.Validators.Clientes;
using PizzeriaAPI.Validators.Gastos;
using PizzeriaAPI.Validators.Pedidos;
using PizzeriaAPI.Validators.Pizzas;
using System.Text;

namespace PizzeriaAPI.Extensions
{
    public static class ServiceExtensions
    {
        public static void ConfigurarBaseDeDatos(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<PizzeriaContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
        }

        public static void ConfigurarJWT(this IServiceCollection services, IConfiguration configuration)
        {
            // Leemos la configuración del appsettings.json
            var key = configuration["Jwt:Key"];
            var issuer = configuration["Jwt:Issuer"];
            var audience = configuration["Jwt:Audience"];

            services.AddAuthentication(options =>
            {
                // JWT es el esquema por defecto
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,           // verifica que el token lo emitió nuestra API
                    ValidateAudience = true,          // verifica que el token es para nuestro frontend
                    ValidateLifetime = true,          // verifica que el token no expiró
                    ValidateIssuerSigningKey = true,  // verifica la firma del token
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
                };
            });

            // Permite usar [Authorize(Roles = "admin")] en los controllers
            services.AddAuthorization();
        }

        public static void ConfigurarServicios(this IServiceCollection services)
        {
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IPizzaService, PizzaService>();
            services.AddScoped<IClienteService, ClienteService>();
            services.AddScoped<IPedidoService, PedidoService>();
            services.AddScoped<IGastoService, GastoService>();
            services.AddScoped<ICategoriaGastoService, CategoriaGastoService>();

            // Validadores de FluentValidation - Login
            services.AddScoped<IValidator<LoginRequestDto>, LoginRequestValidator>();

            // Validadores de FluentValidation - Pizza
            services.AddScoped<IValidator<PizzaRequestDto>, PizzaRequestValidator>();

            // Validadores de FluentValidation - Cliente
            services.AddScoped<IValidator<ClienteRequestDto>, ClienteRequestValidator>();

            // Validadores de FluentValidation - Pedidos
            services.AddScoped<IValidator<DetallePedidoRequestDto>, DetallePedidoRequestValidator>();
            services.AddScoped<IValidator<PedidoRequestDto>, PedidoRequestValidator>();

            // Validador de FluentValidation - Gastos
            services.AddScoped<IValidator<GastoRequestDto>, GastoRequestValidator>();
            services.AddScoped<IValidator<CategoriaGastoRequestDto>,CategoriaGastoRequestValidator>();
        }

        // CORS
        public static void ConfigurarCors(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("PizzeriaPolicy", policy =>
                {
                    policy.WithOrigins("http://localhost:5173") // puerto default de Vite/React
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });
        }
    }
}