using Microsoft.Data.SqlClient;

namespace SistemaDePeliculaSakila.Data;

public class FabricaConexion
{
    private readonly string _cadenaConexion;

    public FabricaConexion(IConfiguration configuracion)
    {
        _cadenaConexion = configuracion.GetConnectionString("Sakila")
            ?? throw new InvalidOperationException("Falta la cadena de conexión 'Sakila'.");
    }

    public SqlConnection Crear() => new(_cadenaConexion);
}
