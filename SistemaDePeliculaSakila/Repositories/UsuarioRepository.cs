using Microsoft.Data.SqlClient;
using SistemaDePeliculaSakila.Data;
using SistemaDePeliculaSakila.Models;
using SistemaDePeliculaSakila.Services;

namespace SistemaDePeliculaSakila.Repositories;

public class UsuarioRepository
{
    private readonly FabricaConexion _fabrica;

    public UsuarioRepository(FabricaConexion fabrica) => _fabrica = fabrica;

    // Devuelve el usuario si las credenciales son correctas; null en caso contrario.
    public UsuarioSesion? Autenticar(string nombreUsuario, string contrasena)
    {
        const string sql = @"SELECT u.usuario_id, u.username, u.password_hash, u.rol, u.customer_id, u.staff_id,
                                    COALESCE(c.first_name + ' ' + c.last_name,
                                             s.first_name + ' ' + s.last_name, u.username) AS nombre
                             FROM usuarios u
                             LEFT JOIN customer c ON c.customer_id = u.customer_id
                             LEFT JOIN staff s ON s.staff_id = u.staff_id
                             WHERE u.username = @usuario AND u.activo = 1";

        using var conexion = _fabrica.Crear();
        conexion.Open();
        using var cmd = new SqlCommand(sql, conexion);
        cmd.Parameters.AddWithValue("@usuario", nombreUsuario.Trim());

        using var lector = cmd.ExecuteReader();
        if (!lector.Read() || !ProtectorContrasena.Verificar(contrasena, lector.Texto("password_hash")))
        {
            return null;
        }

        return new UsuarioSesion(
            lector.Entero("usuario_id"),
            lector.Texto("username"),
            lector.Texto("nombre"),
            lector.Texto("rol"),
            lector.EnteroNulo("customer_id"),
            lector.EnteroNulo("staff_id"));
    }

    // Crea el cliente en Sakila y su cuenta de acceso en una sola transacción.
    public ResultadoOperacion Registrar(RegistroViewModel modelo)
    {
        using var conexion = _fabrica.Crear();
        conexion.Open();
        using var transaccion = conexion.BeginTransaction();

        try
        {
            using (var cmd = new SqlCommand("SELECT COUNT(*) FROM usuarios WHERE username = @usuario", conexion, transaccion))
            {
                cmd.Parameters.AddWithValue("@usuario", modelo.NombreUsuario.Trim());
                if (Convert.ToInt32(cmd.ExecuteScalar()) > 0)
                {
                    transaccion.Rollback();
                    return ResultadoOperacion.Error("Ese nombre de usuario ya está en uso.");
                }
            }

            int clienteId;
            using (var cmd = new SqlCommand(
                @"INSERT INTO customer (store_id, first_name, last_name, email, address_id, active, create_date, last_update)
                  SELECT s.store_id, @nombre, @apellido, @correo,
                         (SELECT TOP 1 address_id FROM address WHERE address = 'Direccion no registrada'),
                         'Y', GETDATE(), GETDATE()
                  FROM store s WHERE s.store_id = @tiendaId;
                  SELECT CAST(SCOPE_IDENTITY() AS INT);",
                conexion, transaccion))
            {
                cmd.Parameters.AddWithValue("@nombre", modelo.Nombre.Trim().ToUpperInvariant());
                cmd.Parameters.AddWithValue("@apellido", modelo.Apellido.Trim().ToUpperInvariant());
                cmd.Parameters.AddWithValue("@correo", modelo.Correo.Trim());
                cmd.Parameters.AddWithValue("@tiendaId", modelo.TiendaId);

                var resultado = cmd.ExecuteScalar();
                if (resultado is null or DBNull)
                {
                    transaccion.Rollback();
                    return ResultadoOperacion.Error("La tienda seleccionada no existe.");
                }

                clienteId = Convert.ToInt32(resultado);
            }

            using (var cmd = new SqlCommand(
                @"INSERT INTO usuarios (username, password_hash, rol, customer_id)
                  VALUES (@usuario, @hash, @rol, @clienteId)",
                conexion, transaccion))
            {
                cmd.Parameters.AddWithValue("@usuario", modelo.NombreUsuario.Trim());
                cmd.Parameters.AddWithValue("@hash", ProtectorContrasena.Proteger(modelo.Contrasena));
                cmd.Parameters.AddWithValue("@rol", Roles.Cliente);
                cmd.Parameters.AddWithValue("@clienteId", clienteId);
                cmd.ExecuteNonQuery();
            }

            transaccion.Commit();
            return ResultadoOperacion.Ok("Cuenta creada correctamente. Ya puede iniciar sesión.");
        }
        catch
        {
            transaccion.Rollback();
            throw;
        }
    }

    public void RegistrarAcceso(string nombreUsuario, bool exitoso, string? ip)
    {
        const string sql = "INSERT INTO login_log (username, exitoso, ip) VALUES (@usuario, @exitoso, @ip)";

        using var conexion = _fabrica.Crear();
        conexion.Open();
        using var cmd = new SqlCommand(sql, conexion);
        cmd.Parameters.AddWithValue("@usuario", nombreUsuario.Trim());
        cmd.Parameters.AddWithValue("@exitoso", exitoso);
        cmd.Parameters.AddWithValue("@ip", (object?)ip ?? DBNull.Value);
        cmd.ExecuteNonQuery();
    }

    // Usuarios de demostración, creados solo si la tabla está vacía.
    public void CrearUsuariosIniciales()
    {
        using var conexion = _fabrica.Crear();
        conexion.Open();

        using (var cmd = new SqlCommand("SELECT COUNT(*) FROM usuarios", conexion))
        {
            if (Convert.ToInt32(cmd.ExecuteScalar()) > 0)
            {
                return;
            }
        }

        Insertar(conexion, "admin", "Admin123*", Roles.Administrador, null, 1);
        Insertar(conexion, "cliente1", "Cliente123*", Roles.Cliente, 1, null);
        Insertar(conexion, "cliente2", "Cliente123*", Roles.Cliente, 2, null);
    }

    private static void Insertar(SqlConnection conexion, string usuario, string contrasena,
                                 string rol, int? clienteId, int? empleadoId)
    {
        const string sql = @"INSERT INTO usuarios (username, password_hash, rol, customer_id, staff_id)
                             VALUES (@usuario, @hash, @rol, @clienteId, @empleadoId)";

        using var cmd = new SqlCommand(sql, conexion);
        cmd.Parameters.AddWithValue("@usuario", usuario);
        cmd.Parameters.AddWithValue("@hash", ProtectorContrasena.Proteger(contrasena));
        cmd.Parameters.AddWithValue("@rol", rol);
        cmd.Parameters.AddWithValue("@clienteId", (object?)clienteId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@empleadoId", (object?)empleadoId ?? DBNull.Value);
        cmd.ExecuteNonQuery();
    }
}
