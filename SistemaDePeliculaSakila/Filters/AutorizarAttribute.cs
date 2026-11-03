using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SistemaDePeliculaSakila.Extensions;

namespace SistemaDePeliculaSakila.Filters;

// Sin roles: exige sesión iniciada. Con roles: exige además que el rol esté permitido.
public class AutorizarAttribute : ActionFilterAttribute
{
    private readonly string[] _roles;

    public AutorizarAttribute(params string[] roles) => _roles = roles;

    public override void OnActionExecuting(ActionExecutingContext contexto)
    {
        var usuario = contexto.HttpContext.Session.ObtenerUsuario();

        if (usuario is null)
        {
            contexto.Result = new RedirectToActionResult("Login", "Cuenta", null);
            return;
        }

        if (_roles.Length > 0 && !_roles.Contains(usuario.Rol))
        {
            contexto.Result = new RedirectToActionResult("AccesoDenegado", "Cuenta", null);
        }
    }
}
