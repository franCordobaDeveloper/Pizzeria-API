namespace PizzeriaAPI.Models
{
    public class Pedido
    {
        public int Id { get; set; }

        public int ClienteId { get; set; }
        public Cliente Cliente { get; set; }

        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; }

        public string Tipo { get; set; }
        public string Estado { get; set; } // "activo" o "cancelado"
        public string MetodoPago { get; set; }
        public decimal CostoDelivery { get; set; }
        public decimal Total { get; set; }
        public string? Notas { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navegación
        public ICollection<DetallePedido> DetallesPedido { get; set; }
    }
}