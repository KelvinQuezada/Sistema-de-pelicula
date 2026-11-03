using System.Text.Json;
using SistemaDePeliculaSakila.Models;

namespace SistemaDePeliculaSakila.Extensions;

public static class SessionExtensions
{
    private const string ClaveUsuario = "UsuarioActual";

    public static void GuardarUsuario(this ISession sesion, UsuarioSesion usuario) =>
        sesion.SetString(ClaveUsuario, JsonSerializer.Serialize(usuario));

    public static UsuarioSesion? ObtenerUsuario(this ISession sesion)
    {
        var json = sesion.GetString(ClaveUsuario);
        return json is null ? null : JsonSerializer.Deserialize<UsuarioSesion>(json);
    }
}
