using System.ComponentModel.DataAnnotations;

namespace WEB.Models.ViewModels;

public class PaqueteViewModel
{
    public int PaqueteId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal PrecioBase { get; set; }
    public int MaxPersonas { get; set; }
    public int DuracionHoras { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
}

public class PaqueteFormViewModel
{
    public int PaqueteId { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [MaxLength(100, ErrorMessage = "Máximo 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(500, ErrorMessage = "Máximo 500 caracteres.")]
    public string? Descripcion { get; set; }

    [Required(ErrorMessage = "El precio base es obligatorio.")]
    [Range(0, 9999999.99, ErrorMessage = "Ingrese un precio válido.")]
    public decimal PrecioBase { get; set; }

    [Required(ErrorMessage = "El máximo de personas es obligatorio.")]
    [Range(1, 10000, ErrorMessage = "Debe ser al menos 1 persona.")]
    public int MaxPersonas { get; set; } = 50;

    [Required(ErrorMessage = "La duración es obligatoria.")]
    [Range(1, 72, ErrorMessage = "Ingrese entre 1 y 72 horas.")]
    public int DuracionHoras { get; set; } = 4;
}