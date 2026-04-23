public class GastoRequestDto
{
    public int CategoriaId { get; set; }
    public string? Descripcion { get; set; }  
    public decimal Monto { get; set; }
    public DateOnly Fecha { get; set; }
}