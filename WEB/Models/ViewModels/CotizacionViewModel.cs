using System.ComponentModel.DataAnnotations;

namespace WEB.Models.ViewModels;

public class CotizacionViewModel
{
    public int CotizacionId { get; set; }

    public int EventoId { get; set; }

    public string ClienteNombre { get; set; } = string.Empty;

    public string TipoEvento { get; set; } = string.Empty;

    public string PaqueteNombre { get; set; } = string.Empty;

    public decimal Total { get; set; }

    public string Estado { get; set; } = string.Empty;

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaVencimiento { get; set; }

    public string UsuarioNombre { get; set; } = string.Empty;

    public bool TieneContrato { get; set; }

    public int? ContratoId { get; set; }
}

public class CotizacionFormViewModel
{
    public int CotizacionId { get; set; }

    [Required(ErrorMessage = "La reserva es obligatoria.")]
    public int EventoId { get; set; }

    [Required(ErrorMessage = "El total es obligatorio.")]
    [Range(
        0.01,
        9999999.99,
        ErrorMessage = "El total debe ser mayor a cero.")]
    public decimal Total { get; set; }

    [Required(ErrorMessage = "El estado es obligatorio.")]
    public string Estado { get; set; } = "Borrador";

    [MaxLength(
        500,
        ErrorMessage = "Máximo 500 caracteres.")]
    public string? MotivoRechazo { get; set; }

    public DateTime? FechaVencimiento { get; set; }

    public List<ReservaOpcionViewModel> Reservas { get; set; }
        = new();
}

public class ReservaOpcionViewModel
{
    public int EventoId { get; set; }

    public string ClienteNombre { get; set; } = string.Empty;

    public string TipoEvento { get; set; } = string.Empty;

    public DateTime FechaEvento { get; set; }

    public decimal MontoTotal { get; set; }
}