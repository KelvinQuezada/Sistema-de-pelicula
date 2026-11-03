namespace SistemaDePeliculaSakila.Models;

public record ResultadoOperacion(bool Exito, string Mensaje)
{
    public static ResultadoOperacion Ok(string mensaje) => new(true, mensaje);
    public static ResultadoOperacion Error(string mensaje) => new(false, mensaje);
}
