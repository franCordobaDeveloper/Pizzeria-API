namespace PizzeriaAPI.DTOs.Pedidos
{
    public class DetallePedidoRequestDto
    {
        public int PizzaMitad1Id { get; set; }
        public int? PizzaMitad2Id { get; set; }  // Nullable
        public string Tipo { get; set; } = string.Empty;  // "entera", "combo", "media"
        public int Cantidad { get; set; }
    }
}
