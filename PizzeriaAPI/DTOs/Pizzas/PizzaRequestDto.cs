using FluentValidation;

namespace PizzeriaAPI.DTOs.Pizzas
{
    public class PizzaRequestDto
    {
        public string Nombre { get; set; }
        public decimal PrecioEntera { get; set; }
        public decimal PrecioMitad { get; set; }
    }

   
}
