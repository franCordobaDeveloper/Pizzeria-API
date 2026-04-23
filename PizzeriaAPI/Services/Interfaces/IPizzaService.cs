using PizzeriaAPI.DTOs.Pizzas;

namespace PizzeriaAPI.Services.Interfaces
{
    public interface IPizzaService
    {
        Task<List<PizzaResponseDto>> ObtenerTodasLasPizzasAsync();
        Task<PizzaResponseDto?> ObtenerPizzaPorIdAsync(int idPizza);
        Task<PizzaResponseDto> CrearPizzaAsync(PizzaRequestDto nuevaPizza);
        Task<PizzaResponseDto?> ActualizarPizzaPorIdAsync(int idPizza, PizzaRequestDto request);
        Task<bool> EliminarPizzaPorIdAsync(int idPizza);
    }
}
