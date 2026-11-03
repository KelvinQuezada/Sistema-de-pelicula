namespace SistemaDePeliculaSakila.Models;

public class PeliculaCatalogo
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public int? Anio { get; set; }
    public string Clasificacion { get; set; } = string.Empty;
    public int? Duracion { get; set; }
    public decimal Tarifa { get; set; }
    public int TotalCopias { get; set; }
    public int CopiasDisponibles { get; set; }

    public bool Disponible => CopiasDisponibles > 0;
}
