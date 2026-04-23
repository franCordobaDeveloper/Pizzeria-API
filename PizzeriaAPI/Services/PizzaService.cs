using Microsoft.EntityFrameworkCore;
using PizzeriaAPI.Data;
using PizzeriaAPI.DTOs.Pizzas;
using PizzeriaAPI.Models;
using PizzeriaAPI.Services.Interfaces;

namespace PizzeriaAPI.Services
{
    public class PizzaService : IPizzaService
    {
        private readonly PizzeriaContext _pizzeriaContext;
        private readonly ILogger<PizzaService> _logger;

        public PizzaService(PizzeriaContext pizzeriaContext, ILogger<PizzaService> logger)
        {
            _pizzeriaContext = pizzeriaContext;
            _logger           = logger;
        }
        public async Task<PizzaResponseDto?> ActualizarPizzaPorIdAsync(int idPizza, PizzaRequestDto request)
        {
            if (idPizza <= 0 || request == null) return null;

            try
            {
             
                var actualizarPizza = await _pizzeriaContext.Pizzas
                    .Where(p => p.Id == idPizza)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(p => p.Nombre, request.Nombre)
                        .SetProperty(p => p.PrecioEntera, request.PrecioEntera)
                        .SetProperty(p => p.PrecioMitad, request.PrecioMitad)
                    );

                if (actualizarPizza == 0)
                {
                    _logger?.LogWarning("No se encontró pizza ID {IdPizza} para actualizar", idPizza);
                    return null;
                }

                _logger?.LogInformation("Pizza ID {IdPizza} actualizada exitosamente", idPizza);

                // Devuelvo el DTO actualizado
                return new PizzaResponseDto
                {
                    Id = idPizza,
                    Nombre = request.Nombre,
                    PrecioEntera = request.PrecioEntera,
                    PrecioMitad = request.PrecioMitad
                };
            }
            catch (DbUpdateException dbEx)
            {
                _logger?.LogError(dbEx, "Error BD al actualizar pizza ID {IdPizza}", idPizza);
                throw new Exception("Error al actualizar la pizza", dbEx);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al actualizar pizza ID {IdPizza}", idPizza);
                throw;
            }
        }

        public async Task<PizzaResponseDto> CrearPizzaAsync(PizzaRequestDto nuevaPizza)
        {
            
            if (nuevaPizza == null)
            {
                throw new ArgumentNullException(nameof(nuevaPizza), "Los datos de la pizza no pueden ser nulos");
            }

            try
            {
                // Crear el model con los datos del request
                var pizza = new Pizza
                {
                    Nombre = nuevaPizza.Nombre,
                    PrecioEntera = nuevaPizza.PrecioEntera,
                    PrecioMitad = nuevaPizza.PrecioMitad,
                   
                };

                // Agregar a la DB
                await _pizzeriaContext.Pizzas.AddAsync(pizza);

                // Guardar — EF actualiza pizza.Id automáticamente
                await _pizzeriaContext.SaveChangesAsync();

                _logger.LogInformation("Pizza creada exitosamente - ID: {Id}, Nombre: {Nombre}", pizza.Id, pizza.Nombre);

                return new PizzaResponseDto
                {
                    Id = pizza.Id,
                    Nombre = pizza.Nombre,
                    PrecioEntera = pizza.PrecioEntera,
                    PrecioMitad = pizza.PrecioMitad
                };
            }
            catch (DbUpdateException dbEx)
            {
                _logger?.LogError(dbEx, "Error de BD al crear pizza - Nombre: {Nombre}", nuevaPizza.Nombre);

                
                if (dbEx.InnerException?.Message.Contains("duplicate") == true ||
                    dbEx.InnerException?.Message.Contains("unique") == true)
                {
                    throw new Exception("Ya existe una pizza con ese nombre", dbEx);
                }

                throw new Exception("Error al guardar la pizza en la base de datos", dbEx);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al crear pizza - Nombre: {Nombre}", nuevaPizza.Nombre);
                throw new Exception("Ocurrió un error al crear la pizza. Por favor, intente más tarde.", ex);
            }
        }

        public async Task<bool> EliminarPizzaPorIdAsync(int idPizza)
        {
            if (idPizza <= 0)
            {
                _logger.LogWarning("Se intentó eliminar pizza con ID inválido: {IdPizza}", idPizza);
                return false;
            }

            try
            {
                var eliminados = await _pizzeriaContext.Pizzas
                    .Where(p => p.Id == idPizza)
                    .ExecuteDeleteAsync();

                if (eliminados == 0)
                {
                    _logger.LogWarning("No se encontró pizza con ID {IdPizza} para eliminar", idPizza);
                    return false;
                }

                _logger.LogInformation("Pizza con ID {IdPizza} eliminada exitosamente", idPizza);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar pizza con ID {IdPizza}", idPizza);
                throw new Exception("Ocurrió un error al eliminar la pizza. Por favor, intente más tarde.", ex);
            }
        }

        public async Task<PizzaResponseDto?> ObtenerPizzaPorIdAsync(int idPizza)
        {
            
            if (idPizza <= 0)
            {
                _logger?.LogWarning("Se intentó buscar pizza con ID inválido: {IdPizza}", idPizza);
                return null;
            }

            try
            {
                var pizzaPorId = await _pizzeriaContext.Pizzas
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == idPizza);


                if (pizzaPorId == null)
                {
                    _logger?.LogWarning("No se encontró la pizza con ID {IdPizza}", idPizza);
                    return null;
                }

                return new PizzaResponseDto
                {
                    Id = pizzaPorId.Id,
                    Nombre = pizzaPorId.Nombre,
                    PrecioEntera = pizzaPorId.PrecioEntera,
                    PrecioMitad = pizzaPorId.PrecioMitad
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error inesperado al obtener la pizza con ID {IdPizza}", idPizza);
                throw new Exception("Ocurrió un error al cargar la pizza. Por favor, intente más tarde.", ex);
            }
        }

        public async Task<List<PizzaResponseDto>> ObtenerTodasLasPizzasAsync()
        {
            try
            {
                
                var pizzas = await _pizzeriaContext.Pizzas
                    .AsNoTracking() 
                    .Select(p => new PizzaResponseDto
                    {
                        Id = p.Id,
                        Nombre = p.Nombre,
                        PrecioEntera = p.PrecioEntera,
                        PrecioMitad = p.PrecioMitad
                    })
                    .ToListAsync();

                if (pizzas == null || pizzas.Count == 0)
                {
                    _logger.LogWarning("No se encontraron pizzas en la base de datos");
                    return new List<PizzaResponseDto>();
                }

                _logger.LogInformation("Se obtuvieron {Cantidad} pizzas exitosamente", pizzas.Count);
                return pizzas;
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Error de conexión a la base de datos al obtener pizzas");
                throw new Exception("Error de conexión con la base de datos", dbEx);
            }
            catch (OperationCanceledException cancelEx)
            {
                _logger.LogWarning(cancelEx, "La operación fue cancelada al obtener pizzas");
                throw;  
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al obtener las pizzas");
                throw new Exception("Ocurrió un error al cargar las pizzas. Por favor, intente más tarde.", ex);
            }
        }
    }
}
