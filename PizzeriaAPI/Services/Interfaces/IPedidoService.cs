using PizzeriaAPI.DTOs.Pedidos;

namespace PizzeriaAPI.Services.Interfaces
{
    public interface IPedidoService
    {
        Task<PedidoResponseDto> CrearPedidoAsync(PedidoRequestDto request, int usuarioId);
        Task<List<PedidoResponseDto>> ObtenerPedidosActivosAsync();
        Task<PedidoResponseDto?> ObtenerPedidoPorIdAsync(int id);
        Task<bool> CancelarPedidoAsync(int id);
        Task<PedidoResponseDto?> ActualizarPedidoAsync(int id, PedidoRequestDto request, int usuarioId);
    }
}
