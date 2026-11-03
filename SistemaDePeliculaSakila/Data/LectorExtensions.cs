using Microsoft.Data.SqlClient;

namespace SistemaDePeliculaSakila.Data;

// Convert.* tolera diferencias de tipo entre versiones de Sakila (smallint/int, varchar/int...).
public static class LectorExtensions
{
    public static int Entero(this SqlDataReader lector, string columna) =>
        Convert.ToInt32(lector[columna]);

    public static int? EnteroNulo(this SqlDataReader lector, string columna) =>
        lector[columna] is DBNull ? null : Convert.ToInt32(lector[columna]);

    public static string Texto(this SqlDataReader lector, string columna) =>
        lector[columna] is DBNull ? string.Empty : Convert.ToString(lector[columna])!;

    public static decimal Moneda(this SqlDataReader lector, string columna) =>
        Convert.ToDecimal(lector[columna]);

    public static DateTime Fecha(this SqlDataReader lector, string columna) =>
        Convert.ToDateTime(lector[columna]);

    public static DateTime? FechaNula(this SqlDataReader lector, string columna) =>
        lector[columna] is DBNull ? null : Convert.ToDateTime(lector[columna]);
}
