using Microsoft.EntityFrameworkCore;
using PizzeriaAPI.Data;
using PizzeriaAPI.DTOs.Gastos;
using PizzeriaAPI.Models;
using PizzeriaAPI.Services.Interfaces;

namespace PizzeriaAPI.Services
{
    public class CategoriaGastoService : ICategoriaGastoService
    {
        
        private readonly PizzeriaContext _pizzeriaContext;
        private readonly ILogger<CategoriaGastoService> _logger;

        public CategoriaGastoService(PizzeriaContext pizzeriaContext, ILogger<CategoriaGastoService> logger)
        {
            _pizzeriaContext = pizzeriaContext;
            _logger = logger;
        }
        public async Task<CategoriaGastoResponseDto?> ActualizarCategoriaAsync(int id, CategoriaGastoRequestDto request)
        {
            if (request == null) 
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (id <= 0) 
            {
                throw new ArgumentNullException(nameof(id));
            }

            try
            {
                var actulizarCategoria = await _pizzeriaContext.CategoriasGasto
                .Where(c => c.Id == id && c.Activo == true)
                .ExecuteUpdateAsync(setters => setters.SetProperty(p => p.Nombre, request.Nombre.Trim()));

                if (actulizarCategoria == 0)
                {
                    _logger.LogWarning("No se encontró una categoria activa con ID: {id} para actualizar", id);
                    return null;
                }

                return new CategoriaGastoResponseDto
                {
                    Id = id,
                    Nombre = request.Nombre.Trim(),
                    Activo = true
                };
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Error inesperado al actualizar la categoria con ID: {id}", id);
                throw new Exception("Ocurrió un error inesperado al actualizar la categoria", ex);
            }

            
        }

        public async Task<CategoriaGastoResponseDto> CrearCategoriaAsync(CategoriaGastoRequestDto request)
        {
            if(request == null) throw new ArgumentNullException(nameof(request));

            try
            {
                var categoriaExistente = await _pizzeriaContext.CategoriasGasto
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Nombre.ToLower() == request.Nombre.Trim().ToLower());
                
                if (categoriaExistente != null)
                {
                    _logger.LogWarning("Intento de crear una categoria con un nombre ya existente: {Nombre}", request.Nombre);
                    throw new Exception("Ya existe una categoria con ese Nombre");
                }

                var nuevaCategoria = new CategoriaGasto
                {
                    Nombre = request.Nombre.Trim(),
                };

                await _pizzeriaContext.CategoriasGasto.AddAsync(nuevaCategoria);
                await _pizzeriaContext.SaveChangesAsync();

                _logger.LogInformation("Categoria creada exitosamente con ID: {Id} y Nombre: {Nombre}", nuevaCategoria.Id, nuevaCategoria.Nombre);

                return new CategoriaGastoResponseDto
                {
                    Id = nuevaCategoria.Id,
                    Nombre = nuevaCategoria.Nombre,
                    Activo = true

                };
            }
            catch (DbUpdateException dbEx) 
            {
                _logger.LogError(dbEx, "Error al crear una categoria con el nombre {Nombre}", request.Nombre);
                if (dbEx.InnerException?.Message.Contains("duplicate") == true ||
                    dbEx.InnerException?.Message.Contains("unique") == true)
                {
                    throw new Exception("Ya existe una categoria con ese Nombre", dbEx);
                }

                throw new Exception("Error al guardar el nombre de la categoria en la base de datos", dbEx);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al crear una categoria con el nombre {Nombre}", request.Nombre);
                throw new Exception("Ocurrió un error inesperado al crear la categoria", ex);
            }
        }

        public async Task<bool> EliminarCategoriaAsync(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Intento de desactivar  una categoria con ID no válido: {id}", id);
                return false;
            }

            try
            {
                var actulizarCategoria = await _pizzeriaContext.CategoriasGasto
                    .Where(c => c.Id == id && c.Activo == true)
                    .ExecuteUpdateAsync(setters => setters.SetProperty(p => p.Activo, false));
                
                if(actulizarCategoria == 0)
                {
                    _logger.LogWarning("No se encontró una categoria activa con ID: {id} para desactivar", id);
                    return false;
                }
                _logger.LogInformation("Categoria con ID: {id} desactivada exitosamente", id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al desactivar la categoria con ID: {id}", id);
                throw new Exception("Ocurrió un error inesperado al desactivar la categoria", ex);
            }

        }

        public async Task<List<CategoriaGastoResponseDto>> ObtenerCategoriasActivasAsync()
        {
            try
            {
                var categoriasActivas = await _pizzeriaContext.CategoriasGasto
                .AsNoTracking()
                .Where(c => c.Activo == true)
                .Select(c => new CategoriaGastoResponseDto
                {
                    Id = c.Id,
                    Nombre = c.Nombre
                })
                .ToListAsync();

                _logger.LogInformation("Se obtuvieron {Count} categorias activas", categoriasActivas.Count);

                return categoriasActivas;
            }
            catch (Exception ex) 
            { 
                _logger.LogError(ex, "Error al obtener las categorias activas");
                throw new Exception("Ocurrió un error inesperado al obtener las categorias activas", ex);
            }
        }
    }
}
