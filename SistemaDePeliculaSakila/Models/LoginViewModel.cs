using System.ComponentModel.DataAnnotations;

namespace SistemaDePeliculaSakila.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "Ingrese su usuario.")]
    [StringLength(50)]
    [Display(Name = "Usuario")]
    public string NombreUsuario { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingrese su contraseña.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Contrasena { get; set; } = string.Empty;
}
