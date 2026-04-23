using System.ComponentModel.DataAnnotations.Schema;

namespace PizzeriaAPI.Models
{
    public class DetallePedido
    {
        public int Id { get; set; }

        public int PedidoId { get; set; }
        public Pedido Pedido { get; set; }

        public int PizzaMitad1Id { get; set; }
        public Pizza PizzaMitad1 { get; set; }

        public int? PizzaMitad2Id { get; set; }
        public Pizza? PizzaMitad2 { get; set; }

        public string? Tipo { get; set; } = string.Empty; 

        public decimal PrecioUnitario { get; set; }
        public int Cantidad { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public decimal Subtotal { get; set; }
    }
}