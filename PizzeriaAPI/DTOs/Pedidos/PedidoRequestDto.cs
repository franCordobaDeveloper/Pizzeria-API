namespace PizzeriaAPI.DTOs.Pedidos
{
    public class PedidoRequestDto
    {
        public int ClienteId { get; set; }
        public string Tipo { get; set; }
        public string MetodoPago { get; set; }
        public string? Notas { get; set; }
        public List<DetallePedidoRequestDto> Pizzas { get; set; }
    }
}
