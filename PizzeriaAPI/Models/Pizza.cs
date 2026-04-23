namespace PizzeriaAPI.Models
{
    public class Pizza
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public decimal PrecioEntera { get; set; }
        public decimal PrecioMitad { get; set; }

        // Navegación — todos los detalles donde esta pizza es mitad1
        // Navegación — detalles donde esta pizza es mitad1
        public ICollection<DetallePedido> DetallesComoMitad1 { get; set; } = new List<DetallePedido>();

        // Navegación — detalles donde esta pizza es mitad2 (puede ser null)
        public ICollection<DetallePedido> DetallesComoMitad2 { get; set; } = new List<DetallePedido>();
    }
}