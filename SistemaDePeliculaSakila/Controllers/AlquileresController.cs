using Microsoft.AspNetCore.Mvc;
using SistemaDePeliculaSakila.Extensions;
using SistemaDePeliculaSakila.Filters;
using SistemaDePeliculaSakila.Models;
using SistemaDePeliculaSakila.Repositories;

namespace SistemaDePeliculaSakila.Controllers;

[Autorizar]
public class AlquileresController : Controller
{
    private readonly AlquilerRepository _alquileres;

    public AlquileresController(AlquilerRepository alquileres) => _alquileres = alquileres;

    // El cliente se obtiene de la sesión; nunca se recibe desde el formulario.
    [HttpPost]
    [Autorizar(Roles.Cliente)]
    public IActionResult Alquilar(int peliculaId)
    {
        var usuario = HttpContext.Session.ObtenerUsuario()!;
        var resultado = _alquileres.Alquilar(usuario.CustomerId!.Value, peliculaId);

        TempData[resultado.Exito ? "Exito" : "Error"] = resultado.Mensaje;
        return resultado.Exito
            ? RedirectToAction(nameof(MisAlquileres))
            : RedirectToAction("Detalle", "Peliculas", new { id = peliculaId });
    }

    [Autorizar(Roles.Cliente)]
    public IActionResult MisAlquileres()
    {
        var usuario = HttpContext.Session.ObtenerUsuario()!;
        return View(_alquileres.ObtenerPorCliente(usuario.CustomerId!.Value));
    }

    [HttpPost]
    [Autorizar(Roles.Cliente)]
    public IActionResult Devolver(int alquilerId)
    {
        var usuario = HttpContext.Session.ObtenerUsuario()!;
        var resultado = _alquileres.Devolver(alquilerId, usuario.CustomerId!.Value);

        TempData[resultado.Exito ? "Exito" : "Error"] = resultado.Mensaje;
        return RedirectToAction(nameof(MisAlquileres));
    }

    [Autorizar(Roles.Administrador)]
    public IActionResult Todos(bool soloPendientes = false)
    {
        ViewBag.SoloPendientes = soloPendientes;
        return View(_alquileres.ObtenerRecientes(soloPendientes));
    }
}
