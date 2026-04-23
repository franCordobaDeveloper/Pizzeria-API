namespace PizzeriaAPI.DTOs.Clientes
{
    public class ClienteRequestDto
    {
        public string Nombre { get; set; }
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
        public string? Notas { get; set; }
    }
}
