public class GastoResponseDto
{
    public int Id { get; set; }
    public int CategoriaId { get; set; }
    public string CategoriaNombre { get; set; }  
    public string? Descripcion { get; set; }
    public decimal Monto { get; set; }
    public DateOnly Fecha { get; set; }           
    public string CargadoPor { get; set; }
}