using Azure.Core;
using Microsoft.EntityFrameworkCore;
using PizzeriaAPI.Data;
using PizzeriaAPI.DTOs.Clientes;
using PizzeriaAPI.Models;
using PizzeriaAPI.Services.Interfaces;

namespace PizzeriaAPI.Services
{
    public class ClienteService: IClienteService
    {
        private readonly PizzeriaContext _pizzeriaContext;
        private readonly ILogger<ClienteService> _logger;

        public ClienteService(PizzeriaContext pizzeriaContext, ILogger<ClienteService> logger) 
        {
            _pizzeriaContext = pizzeriaContext;
            _logger = logger;
        }

        public async Task<ClienteResponseDto?> BuscarClientePorTelefonoAsync(string telefono)
        {
            if (string.IsNullOrWhiteSpace(telefono))
            {
                _logger.LogWarning("Intento de búsqueda con teléfono nulo o vacío");
                return null;
            }

            try
            {
                var telefonoNormalizado = telefono.Trim();

                var cliente = await _pizzeriaContext.Clientes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Telefono == telefonoNormalizado);

                if (cliente == null)
                {
                    _logger?.LogInformation("Cliente no encontrado con teléfono: {Telefono}", telefono);
                    return null;
                }

                _logger?.LogInformation("Cliente encontrado: {Nombre} - Teléfono: {Telefono}",
                    cliente.Nombre, cliente.Telefono);

                return new ClienteResponseDto
                {
                    Id = cliente.Id,
                    Nombre = cliente.Nombre,
                    Telefono = cliente.Telefono,
                    Direccion = cliente.Direccion,
                    Notas = cliente.Notas
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al buscar cliente por teléfono: {Telefono}", telefono);
                throw new Exception("Ocurrió un error al buscar el cliente. Por favor, intente más tarde.", ex);
            }
        }

        public async Task<ClienteResponseDto> CrearClienteAsync(ClienteRequestDto clienteRequest)
        {
            if (clienteRequest == null)
                throw new ArgumentNullException(nameof(clienteRequest), "Los datos del cliente no pueden estar vacíos.");

            try
            {
                // Si viene teléfono, buscar si ya existe
                if (!string.IsNullOrEmpty(clienteRequest.Telefono))
                {
                    var clienteExistente = await _pizzeriaContext.Clientes
                        .AsNoTracking()
                        .FirstOrDefaultAsync(c => c.Telefono == clienteRequest.Telefono);

                    if (clienteExistente != null)
                    {
                        _logger.LogInformation("Cliente existente encontrado por teléfono: {Telefono}", clienteRequest.Telefono);
                        return new ClienteResponseDto
                        {
                            Id = clienteExistente.Id,
                            Nombre = clienteExistente.Nombre,
                            Telefono = clienteExistente.Telefono,
                            Direccion = clienteExistente.Direccion,
                            Notas = clienteExistente.Notas
                        };
                    }
                }

                // No existe, crear nuevo
                var cliente = new Cliente
                {
                    Nombre = clienteRequest.Nombre,
                    Telefono = clienteRequest.Telefono,
                    Direccion = clienteRequest.Direccion,
                    Notas = clienteRequest.Notas
                };

                await _pizzeriaContext.Clientes.AddAsync(cliente);
                await _pizzeriaContext.SaveChangesAsync();

                _logger.LogInformation("Cliente creado exitosamente con ID: {Id}", cliente.Id);

                return new ClienteResponseDto
                {
                    Id = cliente.Id,
                    Nombre = cliente.Nombre,
                    Telefono = cliente.Telefono,
                    Direccion = cliente.Direccion,
                    Notas = cliente.Notas
                };
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Error de BD al crear cliente - Nombre: {Nombre}", clienteRequest.Nombre);

                if (dbEx.InnerException?.Message.Contains("duplicate") == true ||
                    dbEx.InnerException?.Message.Contains("unique") == true)
                {
                    throw new Exception("Ya existe un cliente con ese número de teléfono", dbEx);
                }

                throw new Exception("Error al guardar el cliente en la base de datos", dbEx);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al crear cliente - Nombre: {Nombre}", clienteRequest.Nombre);
                throw new Exception("Ocurrió un error al crear el cliente. Por favor, intente más tarde.", ex);
            }
        }

        public async Task<List<ClienteResponseDto>> BuscarClientesAsync(string busqueda)
        {
            if (string.IsNullOrWhiteSpace(busqueda) || busqueda.Trim().Length < 2)
                return new List<ClienteResponseDto>();

            try
            {
                return await _pizzeriaContext.Clientes
                    .AsNoTracking()
                    .Where(c => c.Nombre.Contains(busqueda) || c.Telefono.Contains(busqueda))
                    .Take(5)
                    .Select(c => new ClienteResponseDto
                    {
                        Id = c.Id,
                        Nombre = c.Nombre,
                        Telefono = c.Telefono,
                        Direccion = c.Direccion,
                        Notas = c.Notas
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar clientes: '{Busqueda}'", busqueda);
                throw new Exception("Error al buscar clientes", ex);
            }
        }

    }
}
