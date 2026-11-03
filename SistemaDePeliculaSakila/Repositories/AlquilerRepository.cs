using System.Data;
using Microsoft.Data.SqlClient;
using SistemaDePeliculaSakila.Data;
using SistemaDePeliculaSakila.Models;

namespace SistemaDePeliculaSakila.Repositories;

public class AlquilerRepository
{
    private readonly FabricaConexion _fabrica;

    public AlquilerRepository(FabricaConexion fabrica) => _fabrica = fabrica;

    public ResultadoOperacion Alquilar(int clienteId, int peliculaId)
    {
        using var conexion = _fabrica.Crear();
        conexion.Open();
        using var transaccion = conexion.BeginTransaction(IsolationLevel.Serializable);

        try
        {
            // Regla: un cliente no puede tener dos veces la misma película sin devolver.
            using (var cmd = new SqlCommand(
                @"SELECT COUNT(*) FROM rental r
                  INNER JOIN inventory i ON i.inventory_id = r.inventory_id
                  WHERE r.customer_id = @clienteId AND i.film_id = @peliculaId AND r.return_date IS NULL",
                conexion, transaccion))
            {
                cmd.Parameters.AddWithValue("@clienteId", clienteId);
                cmd.Parameters.AddWithValue("@peliculaId", peliculaId);

                if (Convert.ToInt32(cmd.ExecuteScalar()) > 0)
                {
                    transaccion.Rollback();
                    return ResultadoOperacion.Error("Ya tiene esta película alquilada. Devuélvala antes de alquilarla otra vez.");
                }
            }

            // Regla de disponibilidad: busca una copia sin alquiler pendiente.
            int? inventarioId = null;
            int tiendaId = 0;
            using (var cmd = new SqlCommand(
                @"SELECT TOP 1 i.inventory_id, i.store_id
                  FROM inventory i WITH (UPDLOCK, ROWLOCK)
                  INNER JOIN film f ON f.film_id = i.film_id
                  WHERE f.film_id = @peliculaId AND f.eliminado = 0 AND i.eliminado = 0
                    AND NOT EXISTS (SELECT 1 FROM rental r
                                    WHERE r.inventory_id = i.inventory_id AND r.return_date IS NULL)
                  ORDER BY i.inventory_id",
                conexion, transaccion))
            {
                cmd.Parameters.AddWithValue("@peliculaId", peliculaId);
                using var lector = cmd.ExecuteReader();
                if (lector.Read())
                {
                    inventarioId = lector.Entero("inventory_id");
                    tiendaId = lector.Entero("store_id");
                }
            }

            if (inventarioId is null)
            {
                transaccion.Rollback();
                return ResultadoOperacion.Error("No hay copias disponibles: todas están alquiladas actualmente.");
            }

            // Sakila exige un empleado en cada alquiler: se usa uno de la tienda de la copia.
            using (var cmd = new SqlCommand(
                @"INSERT INTO rental (rental_date, inventory_id, customer_id, staff_id, last_update)
                  SELECT GETDATE(), @inventarioId, @clienteId, MIN(s.staff_id), GETDATE()
                  FROM staff s WHERE s.store_id = @tiendaId
                  HAVING COUNT(*) > 0",
                conexion, transaccion))
            {
                cmd.Parameters.AddWithValue("@inventarioId", inventarioId.Value);
                cmd.Parameters.AddWithValue("@clienteId", clienteId);
                cmd.Parameters.AddWithValue("@tiendaId", tiendaId);

                if (cmd.ExecuteNonQuery() == 0)
                {
                    transaccion.Rollback();
                    return ResultadoOperacion.Error("La tienda de la copia no tiene empleados registrados.");
                }
            }

            transaccion.Commit();
            return ResultadoOperacion.Ok("Alquiler registrado correctamente.");
        }
        catch
        {
            transaccion.Rollback();
            throw;
        }
    }

    public ResultadoOperacion Devolver(int alquilerId, int clienteId)
    {
        using var conexion = _fabrica.Crear();
        conexion.Open();
        using var transaccion = conexion.BeginTransaction();

        try
        {
            int? propietarioId = null;
            DateTime? fechaDevolucion = null;

            using (var cmd = new SqlCommand(
                "SELECT customer_id, return_date FROM rental WITH (UPDLOCK) WHERE rental_id = @alquilerId",
                conexion, transaccion))
            {
                cmd.Parameters.AddWithValue("@alquilerId", alquilerId);
                using var lector = cmd.ExecuteReader();
                if (lector.Read())
                {
                    propietarioId = lector.Entero("customer_id");
                    fechaDevolucion = lector.FechaNula("return_date");
                }
            }

            // Regla de propiedad: solo puede devolver sus propios alquileres.
            if (propietarioId != clienteId)
            {
                transaccion.Rollback();
                return ResultadoOperacion.Error("El alquiler no existe o no le pertenece.");
            }

            // Regla de estado: un alquiler devuelto no puede devolverse otra vez.
            if (fechaDevolucion is not null)
            {
                transaccion.Rollback();
                return ResultadoOperacion.Error("Este alquiler ya fue devuelto anteriormente.");
            }

            // Se actualiza la fecha de devolución; el historial no se elimina.
            using (var cmd = new SqlCommand(
                @"UPDATE rental SET return_date = GETDATE(), last_update = GETDATE()
                  WHERE rental_id = @alquilerId AND customer_id = @clienteId AND return_date IS NULL",
                conexion, transaccion))
            {
                cmd.Parameters.AddWithValue("@alquilerId", alquilerId);
                cmd.Parameters.AddWithValue("@clienteId", clienteId);
                cmd.ExecuteNonQuery();
            }

            transaccion.Commit();
            return ResultadoOperacion.Ok("Devolución registrada correctamente.");
        }
        catch
        {
            transaccion.Rollback();
            throw;
        }
    }

    public List<AlquilerItem> ObtenerPorCliente(int clienteId) =>
        Consultar("WHERE r.customer_id = @clienteId", "ORDER BY r.rental_date DESC", clienteId);

    public List<AlquilerItem> ObtenerRecientes(bool soloPendientes) =>
        Consultar(soloPendientes ? "WHERE r.return_date IS NULL" : string.Empty,
                  "ORDER BY r.rental_date DESC", null, 200);

    // Los fragmentos recibidos son constantes del código, nunca datos del usuario.
    private List<AlquilerItem> Consultar(string filtro, string orden, int? clienteId, int? limite = null)
    {
        var tope = limite is null ? string.Empty : $"TOP {limite}";
        var sql = $@"SELECT {tope} r.rental_id, f.title, c.first_name, c.last_name, r.rental_date, r.return_date
                     FROM rental r
                     INNER JOIN inventory i ON i.inventory_id = r.inventory_id
                     INNER JOIN film f ON f.film_id = i.film_id
                     INNER JOIN customer c ON c.customer_id = r.customer_id
                     {filtro} {orden}";
        var alquileres = new List<AlquilerItem>();

        using var conexion = _fabrica.Crear();
        conexion.Open();
        using var cmd = new SqlCommand(sql, conexion);
        if (clienteId is not null)
        {
            cmd.Parameters.AddWithValue("@clienteId", clienteId.Value);
        }

        using var lector = cmd.ExecuteReader();
        while (lector.Read())
        {
            alquileres.Add(new AlquilerItem
            {
                Id = lector.Entero("rental_id"),
                Pelicula = lector.Texto("title"),
                Cliente = $"{lector.Texto("first_name")} {lector.Texto("last_name")}",
                FechaAlquiler = lector.Fecha("rental_date"),
                FechaDevolucion = lector.FechaNula("return_date")
            });
        }

        return alquileres;
    }
}
