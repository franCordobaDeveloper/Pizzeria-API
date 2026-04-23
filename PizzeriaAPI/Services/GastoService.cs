using PizzeriaAPI.Services.Interfaces;

namespace PizzeriaAPI.Services
{
    public class GastoService : IGastoService
    {
        public Task<GastoResponseDto?> ActualizarGastoAsync(int id, GastoRequestDto gastoRequest)
        {
            throw new NotImplementedException();
        }

        public Task<GastoResponseDto> CrearGastoAsync(GastoRequestDto gastoRequest, int usuarioId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> EliminarGastoAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<List<GastoResponseDto>> ObtenerGastosAsync(DateOnly? fecha)
        {
            throw new NotImplementedException();
        }
    }
}
