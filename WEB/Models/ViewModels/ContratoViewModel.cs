using System.ComponentModel.DataAnnotations;

namespace WEB.Models.ViewModels;

public class ContratoViewModel
{
    public int ContratoId { get; set; }

    public int CotizacionId { get; set; }

    public string NumeroContrato { get; set; } = string.Empty;

    public DateTime FechaContrato { get; set; }

    public string Estado { get; set; } = string.Empty;

    public string? Observaciones { get; set; }

    public string? RutaPdf { get; set; }

    public string ClienteNombre { get; set; } = string.Empty;

    public string ClienteCorreo { get; set; } = string.Empty;

    public string TipoEvento { get; set; } = string.Empty;

    public DateTime FechaEvento { get; set; }

    public string PaqueteNombre { get; set; } = string.Empty;

    public decimal Total { get; set; }

    public string UsuarioNombre { get; set; } = string.Empty;
}

public class ContratoFormViewModel
{
    public int ContratoId { get; set; }

    [Required(ErrorMessage = "La cotización es obligatoria.")]
    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una cotización.")]
    public int CotizacionId { get; set; }

    public string NumeroContrato { get; set; } = string.Empty;

    [Required(ErrorMessage = "El estado es obligatorio.")]
    [MaxLength(20, ErrorMessage = "El estado no puede superar los 20 caracteres.")]
    public string Estado { get; set; } = "Pendiente";

    [MaxLength(
        1000,
        ErrorMessage = "Las observaciones no pueden superar los 1000 caracteres.")]
    public string? Observaciones { get; set; }

    public DateTime FechaContrato { get; set; }

    public List<CotizacionContratoOpcionViewModel> Cotizaciones { get; set; }
        = new();
}

public class CotizacionContratoOpcionViewModel
{
    public int CotizacionId { get; set; }

    public string ClienteNombre { get; set; } = string.Empty;

    public string TipoEvento { get; set; } = string.Empty;

    public DateTime FechaEvento { get; set; }

    public decimal Total { get; set; }

    public string Estado { get; set; } = string.Empty;

    public string TextoOpcion =>
        $"Cotización #{CotizacionId} - {ClienteNombre} - " +
        $"{TipoEvento} - {FechaEvento:dd/MM/yyyy} - ₡{Total:N2}";
}