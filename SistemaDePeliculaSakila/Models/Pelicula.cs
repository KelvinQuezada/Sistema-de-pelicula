using System.ComponentModel.DataAnnotations;

namespace SistemaDePeliculaSakila.Models;

// Modelo del formulario de creación y edición.
public class Pelicula {
    public int Id { get; set; }
    [Required(ErrorMessage = "El título es obligatorio.")]
    [StringLength(128, ErrorMessage = "El título admite máximo 128 caracteres.")]
    [Display(Name = "Título")]
    public string Titulo { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "La descripción admite máximo 1000 caracteres.")]
    [Display(Name = "Descripción")]
    public string? Descripcion { get; set; }

    [Range(1900, 2100, ErrorMessage = "El año debe estar entre 1900 y 2100.")]
    [Display(Name = "Año de lanzamiento")]
    public int AnioLanzamiento { get; set; } = DateTime.Now.Year;

    [Required(ErrorMessage = "Seleccione una clasificación.")]
    [RegularExpression("^(G|PG|PG-13|R|NC-17)$", ErrorMessage = "Clasificación no válida.")]
    [Display(Name = "Clasificación")]
    public string Clasificacion { get; set; } = "PG";

    [Range(1, 600, ErrorMessage = "La duración debe estar entre 1 y 600 minutos.")]
    [Display(Name = "Duración (min)")]
    public int Duracion { get; set; } = 90;

    [Range(1, 30, ErrorMessage = "Los días de alquiler deben estar entre 1 y 30.")]
    [Display(Name = "Días de alquiler")]
    public int DuracionAlquiler { get; set; } = 3;

    [Range(0.99, 99.99, ErrorMessage = "La tarifa debe estar entre 0.99 y 99.99.")]
    [Display(Name = "Tarifa de alquiler")]
    public decimal TarifaAlquiler { get; set; } = 4.99m;

    [Range(0.99, 999.99, ErrorMessage = "El costo de reposición debe estar entre 0.99 y 999.99.")]
    [Display(Name = "Costo de reposición")]
    public decimal CostoReposicion { get; set; } = 19.99m;
}
