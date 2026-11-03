namespace SistemaDePeliculaSakila.Models;

public class CopiaInventario
{
    public int InventarioId { get; set; }
    public int TiendaId { get; set; }
    public bool Alquilada { get; set; }
}

public class InventarioViewModel
{
    public PeliculaCatalogo Pelicula { get; set; } = new();
    public List<CopiaInventario> Copias { get; set; } = [];
    public List<int> Tiendas { get; set; } = [];
}
