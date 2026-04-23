using PizzeriaAPI.DTOs.Gastos;

namespace PizzeriaAPI.Services.Interfaces
{
    public interface ICategoriaGastoService
    {
        Task<List<CategoriaGastoResponseDto>> ObtenerCategoriasActivasAsync();
        Task<CategoriaGastoResponseDto> CrearCategoriaAsync(CategoriaGastoRequestDto request);
        Task<CategoriaGastoResponseDto?> ActualizarCategoriaAsync(int id, CategoriaGastoRequestDto request);
        Task<bool> EliminarCategoriaAsync(int id);
    }
}