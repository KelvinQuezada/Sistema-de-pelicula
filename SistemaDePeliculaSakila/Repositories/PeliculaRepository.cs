using System.Data;
using Microsoft.Data.SqlClient;
using SistemaDePeliculaSakila.Data;
using SistemaDePeliculaSakila.Models;

namespace SistemaDePeliculaSakila.Repositories;

public class PeliculaRepository
{
    // Total de copias activas y copias que no tienen un alquiler pendiente de devolución.
    private const string CopiasTotales =
        "(SELECT COUNT(*) FROM inventory i WHERE i.film_id = f.film_id AND i.eliminado = 0)";

    private const string CopiasDisponibles =
        @"(SELECT COUNT(*) FROM inventory i
           WHERE i.film_id = f.film_id AND i.eliminado = 0
             AND NOT EXISTS (SELECT 1 FROM rental r
                             WHERE r.inventory_id = i.inventory_id AND r.return_date IS NULL))";

    private const string ColumnasCatalogo =
        $@"f.film_id, f.title, f.description, f.release_year, f.rating, f.length, f.rental_rate,
           {CopiasTotales} AS total_copias, {CopiasDisponibles} AS copias_disponibles";

    private readonly FabricaConexion _fabrica;

    public PeliculaRepository(FabricaConexion fabrica) => _fabrica = fabrica;

    public (List<PeliculaCatalogo> Items, int Total) ObtenerCatalogo(string? busqueda, int pagina, int tamano)
    {
        const string filtro = "f.eliminado = 0 AND (@busqueda IS NULL OR f.title LIKE '%' + @busqueda + '%')";
        var items = new List<PeliculaCatalogo>();

        using var conexion = _fabrica.Crear();
        conexion.Open();

        using var cmdTotal = new SqlCommand($"SELECT COUNT(*) FROM film f WHERE {filtro}", conexion);
        AgregarBusqueda(cmdTotal, busqueda);
        var total = Convert.ToInt32(cmdTotal.ExecuteScalar());

        var sql = $@"SELECT {ColumnasCatalogo}
                     FROM film f
                     WHERE {filtro}
                     ORDER BY f.title
                     OFFSET @saltar ROWS FETCH NEXT @tamano ROWS ONLY";
        using var cmd = new SqlCommand(sql, conexion);
        AgregarBusqueda(cmd, busqueda);
        cmd.Parameters.AddWithValue("@saltar", (pagina - 1) * tamano);
        cmd.Parameters.AddWithValue("@tamano", tamano);

        using var lector = cmd.ExecuteReader();
        while (lector.Read())
        {
            items.Add(Mapear(lector));
        }

        return (items, total);
    }

    public PeliculaCatalogo? ObtenerPorId(int id)
    {
        var sql = $"SELECT {ColumnasCatalogo} FROM film f WHERE f.film_id = @id AND f.eliminado = 0";

        using var conexion = _fabrica.Crear();
        conexion.Open();
        using var cmd = new SqlCommand(sql, conexion);
        cmd.Parameters.AddWithValue("@id", id);

        using var lector = cmd.ExecuteReader();
        return lector.Read() ? Mapear(lector) : null;
    }

    public Pelicula? ObtenerParaEditar(int id)
    {
        const string sql = @"SELECT film_id, title, description, release_year, rental_duration,
                                    rental_rate, length, replacement_cost, rating
                             FROM film WHERE film_id = @id AND eliminado = 0";

        using var conexion = _fabrica.Crear();
        conexion.Open();
        using var cmd = new SqlCommand(sql, conexion);
        cmd.Parameters.AddWithValue("@id", id);

        using var lector = cmd.ExecuteReader();
        if (!lector.Read())
        {
            return null;
        }

        return new Pelicula
        {
            Id = lector.Entero("film_id"),
            Titulo = lector.Texto("title"),
            Descripcion = lector.Texto("description"),
            AnioLanzamiento = lector.EnteroNulo("release_year") ?? DateTime.Now.Year,
            DuracionAlquiler = lector.Entero("rental_duration"),
            TarifaAlquiler = lector.Moneda("rental_rate"),
            Duracion = lector.EnteroNulo("length") ?? 0,
            CostoReposicion = lector.Moneda("replacement_cost"),
            Clasificacion = lector.Texto("rating")
        };
    }

    public int Crear(Pelicula pelicula)
    {
        const string sql = @"INSERT INTO film (title, description, release_year, language_id, rental_duration,
                                               rental_rate, length, replacement_cost, rating, last_update)
                             VALUES (@titulo, @descripcion, @anio, 1, @duracionAlquiler,
                                     @tarifa, @duracion, @costo, @clasificacion, GETDATE());
                             SELECT CAST(SCOPE_IDENTITY() AS INT);";

        using var conexion = _fabrica.Crear();
        conexion.Open();
        using var cmd = new SqlCommand(sql, conexion);
        AgregarParametros(cmd, pelicula);

        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public bool Actualizar(Pelicula pelicula)
    {
        const string sql = @"UPDATE film
                             SET title = @titulo, description = @descripcion, release_year = @anio,
                                 rental_duration = @duracionAlquiler, rental_rate = @tarifa,
                                 length = @duracion, replacement_cost = @costo, rating = @clasificacion,
                                 last_update = GETDATE()
                             WHERE film_id = @id AND eliminado = 0";

        using var conexion = _fabrica.Crear();
        conexion.Open();
        using var cmd = new SqlCommand(sql, conexion);
        AgregarParametros(cmd, pelicula);
        cmd.Parameters.AddWithValue("@id", pelicula.Id);

        return cmd.ExecuteNonQuery() > 0;
    }

    // Eliminación lógica: solo si ninguna copia está alquilada en este momento.
    public ResultadoOperacion EliminarLogico(int id)
    {
        const string sql = @"UPDATE film SET eliminado = 1, last_update = GETDATE()
                             WHERE film_id = @id AND eliminado = 0
                               AND NOT EXISTS (SELECT 1 FROM inventory i
                                               INNER JOIN rental r ON r.inventory_id = i.inventory_id
                                               WHERE i.film_id = @id AND r.return_date IS NULL)";

        using var conexion = _fabrica.Crear();
        conexion.Open();
        using var cmd = new SqlCommand(sql, conexion);
        cmd.Parameters.AddWithValue("@id", id);

        return cmd.ExecuteNonQuery() > 0
            ? ResultadoOperacion.Ok("La película fue retirada del catálogo (eliminación lógica).")
            : ResultadoOperacion.Error("No se pudo eliminar: la película no existe o tiene copias alquiladas sin devolver.");
    }

    private static void AgregarBusqueda(SqlCommand cmd, string? busqueda) =>
        cmd.Parameters.Add("@busqueda", SqlDbType.NVarChar, 128).Value =
            string.IsNullOrWhiteSpace(busqueda) ? DBNull.Value : busqueda.Trim();

    private static void AgregarParametros(SqlCommand cmd, Pelicula p)
    {
        cmd.Parameters.AddWithValue("@titulo", p.Titulo.Trim());
        cmd.Parameters.AddWithValue("@descripcion", (object?)p.Descripcion ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@anio", p.AnioLanzamiento);
        cmd.Parameters.AddWithValue("@duracionAlquiler", p.DuracionAlquiler);
        cmd.Parameters.AddWithValue("@tarifa", p.TarifaAlquiler);
        cmd.Parameters.AddWithValue("@duracion", p.Duracion);
        cmd.Parameters.AddWithValue("@costo", p.CostoReposicion);
        cmd.Parameters.AddWithValue("@clasificacion", p.Clasificacion);
    }

    private static PeliculaCatalogo Mapear(SqlDataReader lector) => new()
    {
        Id = lector.Entero("film_id"),
        Titulo = lector.Texto("title"),
        Descripcion = lector.Texto("description"),
        Anio = lector.EnteroNulo("release_year"),
        Clasificacion = lector.Texto("rating"),
        Duracion = lector.EnteroNulo("length"),
        Tarifa = lector.Moneda("rental_rate"),
        TotalCopias = lector.Entero("total_copias"),
        CopiasDisponibles = lector.Entero("copias_disponibles")
    };
}
