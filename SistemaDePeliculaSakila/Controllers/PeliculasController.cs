using Microsoft.AspNetCore.Mvc;
using SistemaDePeliculaSakila.Filters;
using SistemaDePeliculaSakila.Models;
using SistemaDePeliculaSakila.Repositories;

namespace SistemaDePeliculaSakila.Controllers;

[Autorizar]
public class PeliculasController : Controller
{
    private const int PeliculasPorPagina = 12;
    private readonly PeliculaRepository _peliculas;

    public PeliculasController(PeliculaRepository peliculas) => _peliculas = peliculas;

    public IActionResult Index(string? busqueda, int pagina = 1)
    {
        pagina = Math.Max(pagina, 1);
        var (items, total) = _peliculas.ObtenerCatalogo(busqueda, pagina, PeliculasPorPagina);

        return View(new PaginaPeliculas
        {
            Items = items,
            Busqueda = busqueda,
            Pagina = pagina,
            TotalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)PeliculasPorPagina))
        });
    }

    public IActionResult Detalle(int id)
    {
        var pelicula = _peliculas.ObtenerPorId(id);
        return pelicula is null ? NotFound() : View(pelicula);
    }

    [Autorizar(Roles.Administrador)]
    public IActionResult Crear() => View(new Pelicula());

    [HttpPost]
    [Autorizar(Roles.Administrador)]
    public IActionResult Crear(Pelicula pelicula)
    {
        if (!ModelState.IsValid)
        {
            return View(pelicula);
        }

        var id = _peliculas.Crear(pelicula);
        TempData["Exito"] = "Película creada. Agregue copias en el inventario para poder alquilarla.";
        return RedirectToAction("Index", "Inventario", new { peliculaId = id });
    }

    [Autorizar(Roles.Administrador)]
    public IActionResult Editar(int id)
    {
        var pelicula = _peliculas.ObtenerParaEditar(id);
        return pelicula is null ? NotFound() : View(pelicula);
    }

    [HttpPost]
    [Autorizar(Roles.Administrador)]
    public IActionResult Editar(Pelicula pelicula)
    {
        if (!ModelState.IsValid)
        {
            return View(pelicula);
        }

        if (!_peliculas.Actualizar(pelicula))
        {
            return NotFound();
        }

        TempData["Exito"] = "Película actualizada correctamente.";
        return RedirectToAction(nameof(Detalle), new { id = pelicula.Id });
    }

    [HttpPost]
    [Autorizar(Roles.Administrador)]
    public IActionResult Eliminar(int id)
    {
        var resultado = _peliculas.EliminarLogico(id);
        TempData[resultado.Exito ? "Exito" : "Error"] = resultado.Mensaje;
        return RedirectToAction(nameof(Index));
    }
}
