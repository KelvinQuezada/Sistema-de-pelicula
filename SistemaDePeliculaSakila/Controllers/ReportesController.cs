using Microsoft.AspNetCore.Mvc;
using SistemaDePeliculaSakila.Filters;
using SistemaDePeliculaSakila.Models;
using SistemaDePeliculaSakila.Repositories;

namespace SistemaDePeliculaSakila.Controllers;

[Autorizar(Roles.Administrador)]
public class ReportesController : Controller
{
    private readonly ReporteRepository _reportes;

    public ReportesController(ReporteRepository reportes) => _reportes = reportes;

    public IActionResult PeliculasMasAlquiladas() => View(_reportes.ObtenerPeliculasMasAlquiladas());

    public IActionResult Accesos() => View(_reportes.ObtenerUltimosAccesos());
}
