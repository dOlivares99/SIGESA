using System.ComponentModel.DataAnnotations;

namespace WEB.Models.ViewModels;

public class ServicioViewModel
{
    public int ServicioId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public decimal PrecioUnitario { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
}

public class ServicioFormViewModel
{
    public int ServicioId { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [MaxLength(100, ErrorMessage = "Máximo 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "La categoría es obligatoria.")]
    [MaxLength(80, ErrorMessage = "Máximo 80 caracteres.")]
    public string Categoria { get; set; } = string.Empty;

    // Escenario 2 edición: precio debe ser mayor a 0
    [Required(ErrorMessage = "El precio unitario es obligatorio.")]
    [Range(0.01, 9999999.99, ErrorMessage = "El precio debe ser mayor a cero.")]
    public decimal PrecioUnitario { get; set; }
}