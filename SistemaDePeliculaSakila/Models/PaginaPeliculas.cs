namespace SistemaDePeliculaSakila.Models;

public class PaginaPeliculas
{
    public List<PeliculaCatalogo> Items { get; set; } = [];
    public string? Busqueda { get; set; }
    public int Pagina { get; set; }
    public int TotalPaginas { get; set; }
}
