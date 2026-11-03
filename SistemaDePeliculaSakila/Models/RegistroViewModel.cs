using System.ComponentModel.DataAnnotations;

namespace SistemaDePeliculaSakila.Models;

public class RegistroViewModel
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(45, ErrorMessage = "El nombre admite máximo 45 caracteres.")]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio.")]
    [StringLength(45, ErrorMessage = "El apellido admite máximo 45 caracteres.")]
    [Display(Name = "Apellido")]
    public string Apellido { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "Ingrese un correo válido.")]
    [StringLength(50, ErrorMessage = "El correo admite máximo 50 caracteres.")]
    [Display(Name = "Correo electrónico")]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Seleccione una tienda.")]
    [Display(Name = "Tienda preferida")]
    public int TiendaId { get; set; }

    [Required(ErrorMessage = "El usuario es obligatorio.")]
    [StringLength(30, MinimumLength = 4, ErrorMessage = "El usuario debe tener entre 4 y 30 caracteres.")]
    [RegularExpression("^[a-zA-Z0-9_.]+$", ErrorMessage = "Use solo letras, números, punto o guion bajo.")]
    [Display(Name = "Usuario")]
    public string NombreUsuario { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [StringLength(60, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
    [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).+$", ErrorMessage = "La contraseña debe incluir letras y números.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Contrasena { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirme la contraseña.")]
    [Compare(nameof(Contrasena), ErrorMessage = "Las contraseñas no coinciden.")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirmar contraseña")]
    public string ConfirmarContrasena { get; set; } = string.Empty;

    // Solo para poblar la lista desplegable; no se valida ni se confía en él.
    public List<int> Tiendas { get; set; } = [];
}
