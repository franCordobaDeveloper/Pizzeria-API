namespace PizzeriaAPI.DTOs.Pedidos
{
    public class PedidoResponseDto
    {
        public int Id { get; set; }
        public string NombreCliente { get; set; }
        public string Tipo { get; set; }
        public string Estado { get; set; }
        public string MetodoPago { get; set; }
        public decimal CostoDelivery { get; set; }
        public decimal Total { get; set; }
        public string? Notas { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<DetallePedidoResponseDto> Pizzas { get; set; }
    }
}
