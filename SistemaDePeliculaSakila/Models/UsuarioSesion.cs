namespace SistemaDePeliculaSakila.Models;

public record UsuarioSesion(
    int UsuarioId,
    string NombreUsuario,
    string NombreCompleto,
    string Rol,
    int? CustomerId,
    int? StaffId)
{
    public bool EsAdministrador => Rol == Roles.Administrador;
    public bool EsCliente => Rol == Roles.Cliente;
}
