using Microsoft.AspNetCore.Mvc;
using SistemaDePeliculaSakila.Extensions;
using SistemaDePeliculaSakila.Models;
using SistemaDePeliculaSakila.Repositories;

namespace SistemaDePeliculaSakila.Controllers;

public class CuentaController : Controller
{
    private readonly UsuarioRepository _usuarios;
    private readonly InventarioRepository _inventario;

    public CuentaController(UsuarioRepository usuarios, InventarioRepository inventario)
    {
        _usuarios = usuarios;
        _inventario = inventario;
    }

    [HttpGet]
    public IActionResult Login()
    {
        if (HttpContext.Session.ObtenerUsuario() is not null)
        {
            return RedirectToAction("Index", "Peliculas");
        }

        return View(new LoginViewModel());
    }

    [HttpPost]
    public IActionResult Login(LoginViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        var usuario = _usuarios.Autenticar(modelo.NombreUsuario, modelo.Contrasena);
        _usuarios.RegistrarAcceso(modelo.NombreUsuario, usuario is not null, HttpContext.Connection.RemoteIpAddress?.ToString());

        if (usuario is null)
        {
            ModelState.AddModelError(string.Empty, "Usuario o contraseña incorrectos.");
            return View(modelo);
        }

        HttpContext.Session.GuardarUsuario(usuario);
        return RedirectToAction("Index", "Peliculas");
    }

    [HttpGet]
    public IActionResult Registro()
    {
        if (HttpContext.Session.ObtenerUsuario() is not null)
        {
            return RedirectToAction("Index", "Peliculas");
        }

        return View(new RegistroViewModel { Tiendas = _inventario.ObtenerTiendas() });
    }

    // Solo crea cuentas con rol Cliente; el rol nunca se recibe desde el formulario.
    [HttpPost]
    public IActionResult Registro(RegistroViewModel modelo)
    {
        modelo.Tiendas = _inventario.ObtenerTiendas();

        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        var resultado = _usuarios.Registrar(modelo);
        if (!resultado.Exito)
        {
            ModelState.AddModelError(string.Empty, resultado.Mensaje);
            return View(modelo);
        }

        TempData["Exito"] = resultado.Mensaje;
        return RedirectToAction(nameof(Login));
    }

    [HttpPost]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction(nameof(Login));
    }

    public IActionResult AccesoDenegado() => View();
}
