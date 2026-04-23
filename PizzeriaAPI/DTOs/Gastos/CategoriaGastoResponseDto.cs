namespace PizzeriaAPI.DTOs.Gastos
{
    public class CategoriaGastoResponseDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = null!;
        public bool Activo { get; set; }
    }
}
