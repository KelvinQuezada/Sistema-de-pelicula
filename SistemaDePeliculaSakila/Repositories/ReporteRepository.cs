using Microsoft.Data.SqlClient;
using SistemaDePeliculaSakila.Data;
using SistemaDePeliculaSakila.Models;

namespace SistemaDePeliculaSakila.Repositories;

public class ReporteRepository
{
    private readonly FabricaConexion _fabrica;

    public ReporteRepository(FabricaConexion fabrica) => _fabrica = fabrica;

    public List<ReporteItem> ObtenerPeliculasMasAlquiladas()
    {
        const string sql = @"SELECT TOP 10 f.title, COUNT(r.rental_id) AS cantidad
                             FROM film f
                             INNER JOIN inventory i ON i.film_id = f.film_id
                             INNER JOIN rental r ON r.inventory_id = i.inventory_id
                             WHERE f.eliminado = 0
                             GROUP BY f.film_id, f.title
                             ORDER BY cantidad DESC, f.title";
        var items = new List<ReporteItem>();

        using var conexion = _fabrica.Crear();
        conexion.Open();
        using var cmd = new SqlCommand(sql, conexion);
        using var lector = cmd.ExecuteReader();
        while (lector.Read())
        {
            items.Add(new ReporteItem
            {
                Pelicula = lector.Texto("title"),
                CantidadAlquileres = lector.Entero("cantidad")
            });
        }

        return items;
    }

    public List<AccesoItem> ObtenerUltimosAccesos()
    {
        const string sql = "SELECT TOP 100 username, exitoso, ip, fecha FROM login_log ORDER BY fecha DESC, log_id DESC";
        var items = new List<AccesoItem>();

        using var conexion = _fabrica.Crear();
        conexion.Open();
        using var cmd = new SqlCommand(sql, conexion);
        using var lector = cmd.ExecuteReader();
        while (lector.Read())
        {
            items.Add(new AccesoItem
            {
                Usuario = lector.Texto("username"),
                Exitoso = Convert.ToBoolean(lector["exitoso"]),
                Ip = lector.Texto("ip"),
                Fecha = lector.Fecha("fecha")
            });
        }

        return items;
    }
}
