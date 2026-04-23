namespace PizzeriaAPI.Models
{
    public class Gasto
    {
        public int Id { get; set; }

        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; }

        public int CategoriaId { get; set; }
        public CategoriaGasto Categoria { get; set; }

        public string? Descripcion { get; set; }
        public decimal Monto { get; set; }
        public DateOnly Fecha { get; set; }
    }
}