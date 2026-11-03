using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SistemaDePeliculaSakila.Models;

namespace SistemaDePeliculaSakila.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => RedirectToAction("Index", "Peliculas");

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
