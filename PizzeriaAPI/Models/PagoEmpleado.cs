namespace PizzeriaAPI.Models
{
    public class PagoEmpleado
    {
        public int Id { get; set; }

        public int EmpleadoId { get; set; }
        public Empleado Empleado { get; set; }

        public decimal Monto { get; set; }
        public DateOnly FechaPago { get; set; }
    }
}