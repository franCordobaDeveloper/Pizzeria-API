namespace PizzeriaAPI.DTOs.Pedidos
{
    public class DetallePedidoResponseDto
    {
        public string? PizzaMitad1 { get; set; }
        public string? PizzaMitad2 { get; set; }  // Nullable
        public string Tipo { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }
}
