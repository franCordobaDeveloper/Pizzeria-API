namespace PizzeriaAPI.Models
{
    public class Empleado
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Rol { get; set; }
        public decimal SueldoSemanal { get; set; }
        public bool Activo { get; set; }

        // Navegación
        public ICollection<PagoEmpleado> Pagos { get; set; }
    }
}