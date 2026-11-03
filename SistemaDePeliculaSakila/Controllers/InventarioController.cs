using Microsoft.AspNetCore.Mvc;
using SistemaDePeliculaSakila.Filters;
using SistemaDePeliculaSakila.Models;
using SistemaDePeliculaSakila.Repositories;

namespace SistemaDePeliculaSakila.Controllers;

[Autorizar(Roles.Administrador)]
public class InventarioController : Controller
{
    private readonly PeliculaRepository _peliculas;
    private readonly InventarioRepository _inventario;

    public InventarioController(PeliculaRepository peliculas, InventarioRepository inventario)
    {
        _peliculas = peliculas;
        _inventario = inventario;
    }

    public IActionResult Index(int peliculaId)
    {
        var pelicula = _peliculas.ObtenerPorId(peliculaId);
        if (pelicula is null)
        {
            return NotFound();
        }

        return View(new InventarioViewModel
        {
            Pelicula = pelicula,
            Copias = _inventario.ObtenerCopias(peliculaId),
            Tiendas = _inventario.ObtenerTiendas()
        });
    }

    [HttpPost]
    public IActionResult Agregar(int peliculaId, int tiendaId)
    {
        var resultado = _inventario.AgregarCopia(peliculaId, tiendaId);
        TempData[resultado.Exito ? "Exito" : "Error"] = resultado.Mensaje;
        return RedirectToAction(nameof(Index), new { peliculaId });
    }

    [HttpPost]
    public IActionResult Retirar(int inventarioId, int peliculaId)
    {
        var resultado = _inventario.RetirarCopia(inventarioId);
        TempData[resultado.Exito ? "Exito" : "Error"] = resultado.Mensaje;
        return RedirectToAction(nameof(Index), new { peliculaId });
    }
}
