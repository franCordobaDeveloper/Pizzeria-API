namespace PizzeriaAPI.DTOs.Pizzas
{
    public class PizzaResponseDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public decimal PrecioEntera { get; set; }
        public decimal PrecioMitad { get; set; }
    }
}
