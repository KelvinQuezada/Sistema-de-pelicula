using Microsoft.Data.SqlClient;
using SistemaDePeliculaSakila.Data;
using SistemaDePeliculaSakila.Models;

namespace SistemaDePeliculaSakila.Repositories;

public class InventarioRepository
{
    private readonly FabricaConexion _fabrica;

    public InventarioRepository(FabricaConexion fabrica) => _fabrica = fabrica;

    public List<CopiaInventario> ObtenerCopias(int peliculaId)
    {
        const string sql = @"SELECT i.inventory_id, i.store_id,
                                    CASE WHEN EXISTS (SELECT 1 FROM rental r
                                                      WHERE r.inventory_id = i.inventory_id AND r.return_date IS NULL)
                                         THEN 1 ELSE 0 END AS alquilada
                             FROM inventory i
                             WHERE i.film_id = @peliculaId AND i.eliminado = 0
                             ORDER BY i.inventory_id";
        var copias = new List<CopiaInventario>();

        using var conexion = _fabrica.Crear();
        conexion.Open();
        using var cmd = new SqlCommand(sql, conexion);
        cmd.Parameters.AddWithValue("@peliculaId", peliculaId);

        using var lector = cmd.ExecuteReader();
        while (lector.Read())
        {
            copias.Add(new CopiaInventario
            {
                InventarioId = lector.Entero("inventory_id"),
                TiendaId = lector.Entero("store_id"),
                Alquilada = lector.Entero("alquilada") == 1
            });
        }

        return copias;
    }

    public List<int> ObtenerTiendas()
    {
        var tiendas = new List<int>();

        using var conexion = _fabrica.Crear();
        conexion.Open();
        using var cmd = new SqlCommand("SELECT store_id FROM store ORDER BY store_id", conexion);
        using var lector = cmd.ExecuteReader();
        while (lector.Read())
        {
            tiendas.Add(lector.Entero("store_id"));
        }

        return tiendas;
    }

    public ResultadoOperacion AgregarCopia(int peliculaId, int tiendaId)
    {
        const string sql = @"INSERT INTO inventory (film_id, store_id, last_update)
                             SELECT f.film_id, s.store_id, GETDATE()
                             FROM film f CROSS JOIN store s
                             WHERE f.film_id = @peliculaId AND f.eliminado = 0 AND s.store_id = @tiendaId";

        using var conexion = _fabrica.Crear();
        conexion.Open();
        using var cmd = new SqlCommand(sql, conexion);
        cmd.Parameters.AddWithValue("@peliculaId", peliculaId);
        cmd.Parameters.AddWithValue("@tiendaId", tiendaId);

        return cmd.ExecuteNonQuery() > 0
            ? ResultadoOperacion.Ok("Copia agregada al inventario.")
            : ResultadoOperacion.Error("La película o la tienda indicada no existe.");
    }

    // Eliminación lógica de la copia; no se permite retirar una copia que está alquilada.
    public ResultadoOperacion RetirarCopia(int inventarioId)
    {
        const string sql = @"UPDATE inventory SET eliminado = 1, last_update = GETDATE()
                             WHERE inventory_id = @inventarioId AND eliminado = 0
                               AND NOT EXISTS (SELECT 1 FROM rental r
                                               WHERE r.inventory_id = @inventarioId AND r.return_date IS NULL)";

        using var conexion = _fabrica.Crear();
        conexion.Open();
        using var cmd = new SqlCommand(sql, conexion);
        cmd.Parameters.AddWithValue("@inventarioId", inventarioId);

        return cmd.ExecuteNonQuery() > 0
            ? ResultadoOperacion.Ok("Copia retirada del inventario.")
            : ResultadoOperacion.Error("No se puede retirar una copia que está alquilada actualmente.");
    }
}
