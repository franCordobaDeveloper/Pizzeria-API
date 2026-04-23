namespace PizzeriaAPI.Models
{
    public class CategoriaGasto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public bool Activo { get; set; }

        public ICollection<Gasto> Gastos { get; set; }
    }
}
