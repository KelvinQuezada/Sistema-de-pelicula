namespace SistemaDePeliculaSakila.Models;

public class AccesoItem
{
    public string Usuario { get; set; } = string.Empty;
    public bool Exitoso { get; set; }
    public string Ip { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
}
