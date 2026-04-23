using PizzeriaAPI.DTOs.Clientes;

namespace PizzeriaAPI.Services.Interfaces
{
    public interface IClienteService
    {
        Task<ClienteResponseDto> CrearClienteAsync(ClienteRequestDto clienteRequest);
        Task<ClienteResponseDto?> BuscarClientePorTelefonoAsync(string telefono);
        Task<List<ClienteResponseDto>> BuscarClientesAsync(string busqueda);
    }
}
