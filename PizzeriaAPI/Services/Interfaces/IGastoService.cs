using PizzeriaAPI.DTOs.Gastos;

namespace PizzeriaAPI.Services.Interfaces
{
    public interface IGastoService
    {
        Task<List<GastoResponseDto>> ObtenerGastosAsync(DateOnly? fecha);
        Task<GastoResponseDto> CrearGastoAsync(GastoRequestDto gastoRequest, int usuarioId);
        Task<GastoResponseDto?> ActualizarGastoAsync(int id, GastoRequestDto gastoRequest);
        Task<bool> EliminarGastoAsync(int id);
    }
}