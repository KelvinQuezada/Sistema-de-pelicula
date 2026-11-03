namespace SistemaDePeliculaSakila.Models;

public class AlquilerItem
{
    public int Id { get; set; }
    public string Pelicula { get; set; } = string.Empty;
    public string Cliente { get; set; } = string.Empty;
    public DateTime FechaAlquiler { get; set; }
    public DateTime? FechaDevolucion { get; set; }

    public bool Devuelto => FechaDevolucion.HasValue;
}

public class TablaAlquileresViewModel
{
    public List<AlquilerItem> Items { get; set; } = [];
    public bool MostrarCliente { get; set; }
    public bool PermitirDevolver { get; set; }
}
